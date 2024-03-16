namespace CQRSBookingKata.Planning;

public interface IPlanningRepository
{
    IQueryable<ReceptionCheck> Checks { get; }
    IQueryable<RoomServiceDuty> Duties { get; }

    void Add(ReceptionCheck check);
    void Add(RoomServiceDuty duty);

    void DoneCheck(int CheckId, int employeeId);
    void CancelCheck(int CheckId, DateTime cancelDate);
    void DoneDuty(int dutyId, int employeeId);
}