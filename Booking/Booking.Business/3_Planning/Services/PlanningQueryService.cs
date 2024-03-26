
namespace BookingKata.Planning;

public class PlanningQueryService : MessageBusClientBase
{
    private readonly IPlanningRepository planning;
    private readonly ITimeService DateTime;
    private readonly IMessageBus bus;

    public PlanningQueryService(IPlanningRepository planning, ITimeService DateTime, IMessageBus bus) : base(bus)
    {
        this.planning = planning;
        this.DateTime = DateTime;
        this.bus = bus;

        bus.Subscribe(this, default, "NEW BOOKING");

        Notified += (sender, message) =>
        {
            switch (message.Verb)
            {
                case "NEW BOOKING":
                    var m = message.Message;
                    break;

                default:
                    break;
            }
        };
    }


    private int Days(DateTime t0, DateTime? t)
    {
        return (int)((t ?? System.DateTime.MaxValue) - t0).TotalDays;
    }

    public IQueryable<ReceptionCheck> GetReceptionPlanning(int hotelId)
    {
        var now = System.DateTime.UtcNow;

        var nowDayNum = OvernightStay.From(now).DayNum;

        return 

            from check in planning.Checks

            where check.HotelId == hotelId &&
                  nowDayNum == check.EventDayNum

            select check;
    }


    public IQueryable<RoomServiceDuty> GetServiceRoomPlanning(int hotelId)
    {
        var now = System.DateTime.UtcNow;

        var priority = (RoomServiceDuty duty) =>
        {
            var prevDays = Days(duty.FreeTime, now);
            var nextDays = Days(now, duty.BusyTime);

            return duty.FloorNum * (
                nextDays == 0 ? 100_000 //today high priority
                : nextDays == 1 ? 10_000 //tomorrow medium priority
                : (prevDays >= 9 ? 9 : prevDays) * 1_000 //then the more stink the more urgent 
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