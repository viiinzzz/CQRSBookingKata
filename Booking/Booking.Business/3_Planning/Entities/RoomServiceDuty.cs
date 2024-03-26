namespace BookingKata.Planning;

public record RoomServiceDuty(
    DateTime FreeTime,//previous customer
    DateTime? BusyTime,//next customer
    int RoomNum,
    int FloorNum,
    bool TaskDone,
    int HotelId,
    int BookingId,
    int? EmployeeId,
    int DutyId
);