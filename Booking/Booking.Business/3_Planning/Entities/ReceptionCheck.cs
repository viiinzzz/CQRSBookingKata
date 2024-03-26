﻿namespace BookingKata.Planning;

public enum ReceptionEventType
{
    CheckIn = 1,
    CheckOut = -1
}

public record ReceptionCheck(
    int EventDayNum,
    DateTime EventTime,
    ReceptionEventType EventType,
    string CustomerLastName,
    string CustomerFirstName,
    int RoomNum,
    bool TaskDone,
    int HotelId,
    int BookingId,
    DateTime? CancelledDate,
    bool Cancelled,
    int? EmployeeId,
    int CheckId
);