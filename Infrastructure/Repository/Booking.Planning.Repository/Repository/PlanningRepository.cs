namespace BookingKata.Infrastructure.Storage;

public class PlanningRepository(IDbContextFactory factory, IServerContextService serverContext, ITimeService DateTime) : IPlanningRepository, ITransactionable
{
    private readonly BookingPlanningContext _planning = factory.CreateDbContext<BookingPlanningContext>();

    private BookingPlanningContext GetContext()
    {
        var context = factory.CreateDbContext<BookingPlanningContext>();

        var utcNow = DateTime.UtcNow;
        var utcNowDayNum = utcNow.FractionalDayNum();
        var serverContextId = serverContext.Id;

        SetServerContext(new ServerContext(utcNow, utcNowDayNum, serverContextId), scoped: false);

        return context;
    }

    public TransactionContext AsTransaction() => new TransactionContext() * _planning;

    public IQueryable<ReceptionCheck> Checks

        => _planning.Checks
            .AsNoTracking();

    public IQueryable<RoomServiceDuty> Duties

        => _planning.Duties
            .AsNoTracking();

    public IQueryable<ServerContext> ServerContexts

        => _planning.ServerContext
            .AsNoTracking();

    public void Add(ReceptionCheck check)
    {
        var entity = _planning.Checks.Add(check);

        _planning.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public void Add(RoomServiceDuty duty)
    {
        var entity = _planning.Duties.Add(duty);

        _planning.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public void DoneCheck(int checkId, int employeeId, DateTime doneDate)
    {
        var check = _planning.Checks
            .Find(checkId);

        if (check == default)
        {
            throw new InvalidOperationException("checkId not found");
        }

        _planning.Entry(check).State = EntityState.Detached;

        var entity = _planning.Checks.Update(check with { TaskDone = true, EmployeeId = employeeId });
        _planning.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public void CancelCheck(int checkId, DateTime cancelDate)
    {
        var check = _planning.Checks.Find(checkId);

        if (check == default)
        {
            throw new InvalidOperationException("checkId not found");
        }

        _planning.Entry(check).State = EntityState.Detached;

        var entity = _planning.Checks.Update(check with { Cancelled = true, CancelledDate = cancelDate });
        _planning.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public void DoneDuty(int dutyId, int employeeId, DateTime doneDate)
    {
        var duty = _planning.Duties.Find(dutyId);

        if (duty == default)
        {
            throw new InvalidOperationException("dutyId not found");
        }

        _planning.Entry(duty).State = EntityState.Detached;

        var entity = _planning.Duties.Update(duty with { TaskDone = true, EmployeeId = employeeId });
        _planning.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public void CancelDuty(int dutyId, DateTime cancelDate)
    {
        var duty = _planning.Duties.Find(dutyId);

        if (duty == default)
        {
            throw new InvalidOperationException("dutyId not found");
        }

        _planning.Entry(duty).State = EntityState.Detached;

        var entity = _planning.Duties.Update(duty with { Cancelled = true, CancelledDate = cancelDate });
        _planning.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public ServerContext? GetServerContext()
    {
        var context = _planning.ServerContext.Find(serverContext.Id);

        if (context == default)
        {
            return default;
        }

        _planning.Entry(serverContext).State = EntityState.Detached;

        return context;
    }


    public void SetServerContext(ServerContext serverContext)
    {
        //InsertOrUpdate

        var update = _planning.ServerContext
            .Any(serverContext2 => serverContext2.ServerContextId == serverContext.ServerContextId);

        var entity = _planning.Entry(serverContext);

        entity.State = update
            ? EntityState.Modified
            : EntityState.Added;

        _planning.SaveChanges();

        entity.State = EntityState.Detached;
    }

}