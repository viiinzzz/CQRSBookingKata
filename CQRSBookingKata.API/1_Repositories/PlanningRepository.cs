
namespace CQRSBookingKata.API;


public class PlanningRepository(IDbContextFactory factory, ITimeService DateTime) : IPlanningRepository, ITransactionable
{
    private readonly BookingPlanningContext _planning = factory.CreateDbContext<BookingPlanningContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _planning;


    public IQueryable<ReceptionCheck> Checks

        => _planning.Checks
            .AsNoTracking();

    public IQueryable<RoomServiceDuty> Duties

        => _planning.Duties
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

    public void DoneCheck(int checkId, int employeeId, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

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

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public void CancelCheck(int checkId, DateTime cancelDate, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var check = _planning.Checks.Find(checkId);

            if (check == default)
            {
                throw new InvalidOperationException("checkId not found");
            }

            _planning.Entry(check).State = EntityState.Detached;

            var entity = _planning.Checks.Update(check with { Cancelled = true, CancelledDate = cancelDate });
            _planning.SaveChanges();
            entity.State = EntityState.Detached;

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public void DoneDuty(int dutyId, int employeeId, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var duty = _planning.Duties.Find(dutyId);

            if (duty == default)
            {
                throw new InvalidOperationException("dutyId not found");
            }

            _planning.Entry(duty).State = EntityState.Detached;

            var entity = _planning.Duties.Update(duty with { TaskDone = true, EmployeeId = employeeId });
            _planning.SaveChanges();
            entity.State = EntityState.Detached;

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }
}