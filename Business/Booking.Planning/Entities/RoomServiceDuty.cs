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
   * /

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

using System.ComponentModel.DataAnnotations.Schema;

namespace BookingKata.Planning;

public record RoomServiceDuty
(
    DateTime FreeTime = default, //previous customer departure
    DateTime BusyTime = default, //next customer arrival
    double FreeDayNum = default, //previous customer departure expressed in fractional day number
    double BusyDayNum = default, //next customer arrival expressed in fractional day number

    int RoomNum = default,
    int FloorNum = default,
    int HotelId = default,
    int UniqueRoomId = default,
    double Latitude = default,
    double Longitude = default,

    int BookingId = default,
    int? EmployeeId = default,
    bool TaskDone = default,
    bool Cancelled = default,
    DateTime? CancelledDate = default,
    int DutyId = default
)
    : RecordWithValidation, IHavePrimaryKeyAndPosition
{
    protected override void Validate()
    {
        Position =
            this is { Latitude: 0, Longitude: 0 }
                ? default
                : new Position(Latitude, Longitude);
    }

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public long PrimaryKey => DutyId;


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public Position? Position { get; private set; }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }
}



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
