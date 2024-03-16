
namespace CQRSBookingKata.API.Repositories;

public partial class PlanningRepository
{
    public IQueryable<ReceptionCheck> Checks => front.Checks.AsQueryable();
    public IQueryable<RoomServiceDuty> Duties => front.Duties.AsQueryable();


    public void Add(ReceptionCheck check)
    {
        front.Checks.Add(check);
        front.SaveChanges();
    }

    public void Add(RoomServiceDuty duty)
    {
        front.Duties.Add(duty);
        front.SaveChanges();
    }

    public void DoneCheck(int checkId, int employeeId)
    {
        var check = front.Checks.Find(checkId);

        if (check == default)
        {
            throw new InvalidOperationException("checkId not found");
        }

        front.Checks.Update(check with { TaskDone = true, EmployeeId = employeeId });
        front.SaveChanges();
    }

    public void CancelCheck(int checkId, DateTime cancelDate)
    {
        var check = front.Checks.Find(checkId);

        if (check == default)
        {
            throw new InvalidOperationException("checkId not found");
        }

        front.Checks.Update(check with { Cancelled = true, CancelledDate = cancelDate });
        front.SaveChanges();
    }

    public void DoneDuty(int dutyId, int employeeId)
    {
        var duty = front.Duties.Find(dutyId);

        if (duty == default)
        {
            throw new InvalidOperationException("dutyId not found");
        }

        front.Duties.Update(duty with { TaskDone = true, EmployeeId = employeeId });
        front.SaveChanges();
    }
}