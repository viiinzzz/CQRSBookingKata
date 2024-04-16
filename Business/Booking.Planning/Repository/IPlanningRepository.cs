namespace BookingKata.Planning;

public interface IPlanningRepository
{
    IQueryable<ReceptionCheck> Checks { get; }
    IQueryable<RoomServiceDuty> Duties { get; }
    IQueryable<ServerContext> ServerContexts { get; }

    void Add(ReceptionCheck check);
    void Add(RoomServiceDuty duty);

    void DoneCheck(int CheckId, int employeeId, DateTime doneDate);
    void CancelCheck(int CheckId, DateTime cancelDate);
    void DoneDuty(int dutyId, int employeeId, DateTime doneDate);
    void CancelDuty(int dutyId, DateTime cancelDate);

    ServerContext? GetServerContext();
    void SetServerContext(ServerContext serverContext);
}