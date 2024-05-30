/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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


    private static readonly string[] StepsAddReceptionCheck = [$"{nameof(PlanningCommandService)}.{nameof(Add)}{nameof(ReceptionCheck)}"];

    private void Add(ReceptionCheck check)
    {
        if (check.Cancelled)
        {
            return;
        }

        var originator = this.GetType().FullName;

        var hotelGeoProxy = bus.AskResult<GeoProxy>(
            Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id<HotelRef>(check.HotelId),
            originator, StepsAddReceptionCheck);

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


    private static readonly string[] StepsAddRoomServiceDuty = [$"{nameof(PlanningCommandService)}.{nameof(Add)}{nameof(RoomServiceDuty)}"];

    private void Add(RoomServiceDuty duty)
    {
        if (duty.Cancelled)
        {
            return;
        }

        var originator = this.GetType().FullName;

        var hotelGeoProxy = bus.AskResult<GeoProxy>(
            Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id<HotelRef>(duty.HotelId),
            originator, StepsAddRoomServiceDuty);

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