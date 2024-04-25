using BookingKata.Shared;

namespace BookingKata.Planning;

public class PlanningCommandService
(
    IPlanningRepository planning,

    IMessageBus bus,

    IGazetteerService geo,
    ITimeService DateTime,
    IServerContextService server
)
{
    public void PlanForBooking(Booking booking)
    {
        var checkIn = new ReceptionCheck
        {
            EventDayNum = booking.ArrivalDayNum,
            EventTime = booking.ArrivalDate,
            EventType = ReceptionEventType.CheckIn,

            RoomNum = booking.RoomNum,
            FloorNum = booking.FloorNum,
            HotelId = booking.HotelId,
            UniqueRoomId = booking.UniqueRoomId,
            Latitude = booking.Latitude,
            Longitude = booking.Longitude,

            CustomerLastName = booking.LastName,
            CustomerFirstName = booking.FirstName,
            BookingId = booking.BookingId,
        };

        var checkOut = new ReceptionCheck
        {
            EventDayNum = booking.DepartureDayNum,
            EventTime = booking.DepartureDate,
            EventType = ReceptionEventType.CheckOut,

            RoomNum = booking.RoomNum,
            FloorNum = booking.FloorNum,
            HotelId = booking.HotelId,
            UniqueRoomId = booking.UniqueRoomId,
            Latitude = booking.Latitude,
            Longitude = booking.Longitude,

            CustomerLastName = booking.LastName,
            CustomerFirstName = booking.FirstName,
            BookingId = booking.BookingId,
        };


        var duty = new RoomServiceDuty
        {
            FreeTime = booking.DepartureDate,
            FreeDayNum = booking.DepartureDate.FractionalDayNum(),
            BusyTime = System.DateTime.MaxValue,
            BusyDayNum = System.DateTime.MaxValue.FractionalDayNum(),

            RoomNum = booking.RoomNum,
            FloorNum = booking.FloorNum,
            HotelId = booking.HotelId,
            UniqueRoomId = booking.UniqueRoomId,
            Latitude = booking.Latitude,
            Longitude = booking.Longitude,

            BookingId = booking.BookingId,
        };

        Add(checkIn);
        Add(checkOut);
        Add(duty);
    }

    public void CancelPlanForBooking(int bookingId)
    {
        var cancelDate = DateTime.UtcNow;

        var checks = (
            from check in planning.Checks

            where check.BookingId == bookingId &&
                  !check.Cancelled &&
                  !check.TaskDone

            select check
        );

        foreach (var check in checks)
        {
            CancelCheck(check.CheckId, cancelDate);
        }

        var duties = (
            from duty in planning.Duties

            where duty.BookingId == bookingId &&
                  !duty.Cancelled &&
                  !duty.TaskDone

            select duty
        );

        foreach(var duty in duties)
        {
            planning.CancelDuty(duty.DutyId, cancelDate);
        }
    }



    private void Add(ReceptionCheck check)
    {
        if (check.Cancelled)
        {
            return;
        }

        var originator = this.GetType().FullName;

        var hotelGeoProxy = bus.AskResult<GeoProxy>(Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id(check.HotelId), originator);

        if (hotelGeoProxy == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(check.HotelId));
        }

        planning.Add(check);

        geo.CopyToReferer(hotelGeoProxy, check);


        if (check.EventType == ReceptionEventType.CheckIn)
        {
            UpdateDuties(check);
        }
    }

    private void Add(RoomServiceDuty duty)
    {
        if (duty.Cancelled)
        {
            return;
        }

        var originator = this.GetType().FullName;

        var hotelGeoProxy = bus.AskResult<GeoProxy>(Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id(duty.HotelId), originator);

        if (hotelGeoProxy == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(duty.HotelId));
        }

        var nextCheckIn = (
            from check in planning.Checks

            where check.EventType == ReceptionEventType.CheckIn &&
                  check.EventDayNum > duty.FreeDayNum &&
                  check.UniqueRoomId == duty.UniqueRoomId &&
                  !check.Cancelled

            select check
        ).FirstOrDefault();

        if (nextCheckIn != null)
        {
            duty = duty with {
                BusyTime = nextCheckIn.EventTime,
                BusyDayNum = nextCheckIn.EventTime.FractionalDayNum()
            };
        }

        planning.Add(duty);

        geo.CopyToReferer(hotelGeoProxy, duty);
    }

    private void UpdateDuties(ReceptionCheck check)
    {
        if (check.EventType != ReceptionEventType.CheckIn ||
            check.Cancelled)
        {
            return;
        }

        var previousDuty = (
            from duty in planning.Duties

            where duty.FreeDayNum <= check.EventDayNum &&
                  duty.UniqueRoomId == check.UniqueRoomId &&
                  !duty.Cancelled

            orderby duty.FreeDayNum descending

            select duty
        ).FirstOrDefault();

        if (previousDuty == null)
        {
            return;
        }

        planning.SetDutyBusyTime(previousDuty.DutyId, check.EventTime, check.EventDayNum);
    }



    private void CancelCheck(int checkId, System.DateTime cancelDate)
    {
        var checks = (
            from c in planning.Checks

            where c.CheckId == checkId &&
                  !c.Cancelled &&
                  !c.TaskDone

            select c
        );
            
        var cancellable = checks.Any();

        if (!cancellable)
        {
            return;
        }

        var check = planning.CancelCheck(checkId, cancelDate);

        RoomServiceDuty? adjacentDuty;

        if (check.EventType == ReceptionEventType.CheckOut)
        {
            adjacentDuty = ( //next
                    from duty in planning.Duties
                    where duty.FreeDayNum >= check.EventDayNum &&
                          duty.UniqueRoomId == check.UniqueRoomId &&
                          !duty.Cancelled
                    orderby duty.FreeDayNum
                    select duty
                ).FirstOrDefault();
        }
        else //CheckIn
        {
            adjacentDuty = ( //previous
                    from duty in planning.Duties
                    where duty.FreeDayNum <= check.EventDayNum &&
                          duty.UniqueRoomId == check.UniqueRoomId &&
                          !duty.Cancelled
                    orderby duty.FreeDayNum descending
                    select duty
                ).FirstOrDefault();
        }

        if (adjacentDuty == null)
        {
            return;
        }

        planning.CancelDuty(adjacentDuty.DutyId, cancelDate);
    }



    public void DoneCheck(int checkId)
    {
        //TODO - authentication
        var currentEmployeeId = 8888_8888;

        planning.DoneCheck(checkId, currentEmployeeId, DateTime.UtcNow);
    }


    public void DoneDuty(int dutyId)
    {
        //TODO - authentication
        var currentEmployeeId = 8888_8888;

        planning.DoneDuty(dutyId, currentEmployeeId, DateTime.UtcNow);
    }

}