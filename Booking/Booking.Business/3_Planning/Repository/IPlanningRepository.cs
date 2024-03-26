namespace BookingKata.Planning;

public interface IPlanningRepository
{
    IQueryable<ReceptionCheck> Checks { get; }
    IQueryable<RoomServiceDuty> Duties { get; }

    void Add(ReceptionCheck check);
    void Add(RoomServiceDuty duty);

    void DoneCheck(int CheckId, int employeeId, bool scoped);
    void CancelCheck(int CheckId, DateTime cancelDate, bool scoped);
    void DoneDuty(int dutyId, int employeeId, bool scoped);
}