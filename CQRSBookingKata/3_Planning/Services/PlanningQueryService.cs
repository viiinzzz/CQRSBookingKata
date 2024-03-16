using CQRSBookingKata.ThirdParty;

namespace CQRSBookingKata.Planning;

public class PlanningQueryService(IPlanningRepository planning, TimeService DateTime)
{
    private bool SameDay(DateTime t1, DateTime t2) =>

        t1.Year == t2.Year &&
        t1.Month == t2.Month &&
        t1.Day == t2.Day;

    private int Days(DateTime t0, DateTime? t)
    {
        return (int)((t ?? System.DateTime.MaxValue) - t0).TotalDays;
    }

    public IQueryable<ReceptionCheck> GetReceptionPlanning(int hotelId)
    {
        var now = DateTime.UtcNow;

        return 

            from check in planning.Checks

            where check.HotelId == hotelId &&
                  SameDay(now, check.EventTime)

            select check;
    }


    public IQueryable<RoomServiceDuty> GetServiceRoomPlanning(int hotelId)
    {
        var now = DateTime.UtcNow;

        var priority = (RoomServiceDuty duty) =>
        {
            var prevDays = Days(duty.FreeTime, now);
            var nextDays = Days(now, duty.BusyTime);

            return duty.FloorNum * (
                nextDays == 0 ? 100000 //today high priority
                : nextDays == 1 ? 10000 //tomorrow medium priority
                : (prevDays >= 9 ? 9 : prevDays) * 1000 //then the more stink the more urgent 
            );
        };

        return
            
            from duty in planning.Duties

            join check in planning.Checks
                on duty.BookingId equals check.BookingId

            where check.EventType == ReceptionEventType.CheckOut &&
                  check.TaskDone &&
                  !duty.TaskDone

            orderby priority(duty) descending 

            select duty;
    }
}