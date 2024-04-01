

namespace BookingKata.Planning;

public partial class PlanningQueryService
(
    IPlanningRepository planning,

    ITimeService DateTime,
    IServerContextService server
)
{
    public IQueryable<ReceptionCheck> GetReceptionFullPlanning(int hotelId)
    {
        var now = System.DateTime.UtcNow;

        var nowDayNum = OvernightStay.From(now).DayNum;

        return 

            from check in planning.Checks

            where check.HotelId == hotelId &&
                  check.EventDayNum >= nowDayNum

            select check;
    }

    public IQueryable<ReceptionCheck> GetReceptionTodayPlanning(int hotelId)
    {
        var now = System.DateTime.UtcNow;

        var nowDayNum = OvernightStay.From(now).DayNum;

        return 

            from check in planning.Checks

            where check.HotelId == hotelId &&
                  check.EventDayNum >= nowDayNum &&
                  check.EventDayNum <= nowDayNum + 1

            select check;
    }

    public IQueryable<ReceptionCheck> GetReceptionWeekPlanning(int hotelId)
    {
        var now = System.DateTime.UtcNow;

        var nowDayNum = OvernightStay.From(now).DayNum;

        return 

            from check in planning.Checks

            where check.HotelId == hotelId &&
                  check.EventDayNum >= nowDayNum &&
                  check.EventDayNum <= nowDayNum + 7

            select check;
    }

    public IQueryable<ReceptionCheck> GetReceptionMonthPlanning(int hotelId)
    {
        var now = System.DateTime.UtcNow;

        var nowDayNum = OvernightStay.From(now).DayNum;

        return 

            from check in planning.Checks

            where check.HotelId == hotelId &&
                  check.EventDayNum >= nowDayNum &&
                  check.EventDayNum <= nowDayNum + 30

            select check;
    }


    public IQueryable<RoomServiceDuty> GetServiceRoomPlanning(int hotelId)
    {
        var now = DateTime.UtcNow;


        return

            from duty in planning.Duties

            join serverContext in planning.ServerContexts
                on server.Id equals serverContext.ServerContextId

            join check in planning.Checks
                on duty.BookingId equals check.BookingId

            where duty.HotelId == hotelId &&
                  !duty.TaskDone &&

                  check.EventType == ReceptionEventType.CheckOut &&
                  check.TaskDone

            // orderby duty.Priority descending
            orderby ( //the higher floor the more urgent (up-down priority)
                    duty.FloorNum *
                    (
                        //today deadline is high priority - between day+0 and day+1
                        (duty.BusyDayNum - serverContext.UtcNowDayNum) >= 0 &&
                        (duty.BusyDayNum - serverContext.UtcNowDayNum) < 1
                            ? 100_000
                            //tomorrow deadline is medium priority - between day+1 and day+2
                            : (duty.BusyDayNum - serverContext.UtcNowDayNum) >= 1 &&
                              (duty.BusyDayNum - serverContext.UtcNowDayNum) < 2
                                ? 10_000
                                //then the more stink the more urgent
                                : 1_000 * (
                                    (serverContext.UtcNowDayNum - duty.FreeDayNum) < 9 // between day-0 and day-9
                                        ? serverContext.UtcNowDayNum - duty.FreeDayNum
                                        //cap to max days penalty
                                        : 9
                                )
                    )
                ) descending

            select duty; //WithTranslation();
    }
}