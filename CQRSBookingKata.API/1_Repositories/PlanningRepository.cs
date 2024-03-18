
namespace CQRSBookingKata.API;


public class PlanningRepository(IDbContextFactory factory, ITimeService DateTime) : IPlanningRepository
{
    private readonly BookingFrontContext _front = factory.CreateDbContext<BookingFrontContext>();

    public IQueryable<ReceptionCheck> Checks => _front.Checks.AsQueryable();
    public IQueryable<RoomServiceDuty> Duties => _front.Duties.AsQueryable();


    public void Add(ReceptionCheck check)
    {
        _front.Checks.Add(check);
        _front.SaveChanges();
    }

    public void Add(RoomServiceDuty duty)
    {
        _front.Duties.Add(duty);
        _front.SaveChanges();
    }

    public void DoneCheck(int checkId, int employeeId)
    {
        var check = _front.Checks.Find(checkId);

        if (check == default)
        {
            throw new InvalidOperationException("checkId not found");
        }

        _front.Checks.Update(check with { TaskDone = true, EmployeeId = employeeId });
        _front.SaveChanges();
    }

    public void CancelCheck(int checkId, DateTime cancelDate)
    {
        var check = _front.Checks.Find(checkId);

        if (check == default)
        {
            throw new InvalidOperationException("checkId not found");
        }

        _front.Checks.Update(check with { Cancelled = true, CancelledDate = cancelDate });
        _front.SaveChanges();
    }

    public void DoneDuty(int dutyId, int employeeId)
    {
        var duty = _front.Duties.Find(dutyId);

        if (duty == default)
        {
            throw new InvalidOperationException("dutyId not found");
        }

        _front.Duties.Update(duty with { TaskDone = true, EmployeeId = employeeId });
        _front.SaveChanges();
    }
}