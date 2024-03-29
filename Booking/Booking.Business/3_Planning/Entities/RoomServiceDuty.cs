using Microsoft.Linq.Translations;

namespace BookingKata.Planning;

public record RoomServiceDuty(
    DateTime FreeTime, //previous customer departure
    DateTime BusyTime, //next customer arrival
    double FreeDayNum, //previous customer departure expressed in fractional day number
    double BusyDayNum, //next customer arrival expressed in fractional day number
    int RoomNum,
    int FloorNum,
    bool TaskDone,
    int HotelId,
    int BookingId,
    int? EmployeeId,
    int DutyId
)
{
    /*
     * Room Service Planning
     *                                Now
     *                                 ^
     *   occupied                      |     unoccupied
     *   XXXXXXXXXXXXXXXXXX<-----------+--------------->XXXXXXXXXXXXXXX
     *                                 |
     *   <Previous Customer>           |    ?Duty?     <Next Customer>
     *                                 |
     *                     *FreeTime   |               *BusyTime
     *                                 |
     *                     <-prevDays->|<---nextDays--->
     *                         9dmax   ?-1d>-2d>
     *                                 Duty.Priority = f(NextDays, PrevDays)
     */

    // public int Priority => PriorityExp.Evaluate(this);

    //
    // public TimeSpan PrevLapse => PrevLapseExp.Evaluate(this);
    // public TimeSpan NextLapse => NextLapseExp.Evaluate(this);
    //
    //
    //
    // private static readonly CompiledExpression<RoomServiceDuty, TimeSpan> 
    //     
    //     PrevLapseExp =
    //
    //         DefaultTranslationOf<RoomServiceDuty>
    //             .Property(duty => duty.PrevLapse)
    //             .Is(duty => duty.Server.UtcNow - duty.FreeTime);
    //
    //
    // private static readonly CompiledExpression<RoomServiceDuty, TimeSpan>
    //
    //     NextLapseExp =
    //
    //         DefaultTranslationOf<RoomServiceDuty>
    //             .Property(duty => duty.NextLapse)
    //             .Is(duty => duty.BusyTime - duty.Server.UtcNow);


    // private static readonly TimeSpan _0_day = TimeSpan.Zero;
    // private static readonly TimeSpan _1_day = TimeSpan.FromDays(1);
    // private static readonly TimeSpan _2_days = TimeSpan.FromDays(2);
    // private static readonly TimeSpan _9_days = TimeSpan.FromDays(9);
    // //
    // private static readonly CompiledExpression<RoomServiceDuty, int>
    //     
    //     PriorityExp = 
    //         
    //         DefaultTranslationOf<RoomServiceDuty>
    //             .Property(duty => duty.Priority)
    //             .Is(duty =>
    //                 //the higher floor the more urgent (up-down priority)
    //                 duty.FloorNum *
    //                 (
    //                     //today deadline is high priority
    //                     duty.NextLapse >= _0_day &&
    //                     duty.NextLapse < _1_day
    //                     ? 100_000
    //                         //tomorrow deadline is medium priority
    //                         : duty.NextLapse >= _1_day &&
    //                           duty.NextLapse < _2_days
    //                         ? 10_000
    //                             //then the more stink the more urgent
    //                             : 1_000 * (
    //                                 duty.PrevLapse < _9_days 
    //                                 ? duty.PrevLapse.Days
    //                                     //cap to max days penalty
    //                                     : 9
    //                             )
    //                 )
    //             );
}