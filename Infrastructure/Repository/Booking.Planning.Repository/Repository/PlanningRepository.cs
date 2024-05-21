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
 */

namespace BookingKata.Infrastructure.Storage;

public class PlanningRepository
(
    IDbContextFactory factory,
    IServerContextService serverContext, 
    ITimeService DateTime
)
    : IPlanningRepository, ITransactionable
{
    private readonly BookingPlanningContext _planning = factory.CreateDbContext<BookingPlanningContext>();

    private BookingPlanningContext GetContext()
    {
        var context = factory.CreateDbContext<BookingPlanningContext>();

        var utcNow = DateTime.UtcNow;
        var utcNowDayNum = utcNow.FractionalDayNum();
        var serverContextId = serverContext.Id;

        var serverContext2 = new ServerContext(utcNow, utcNowDayNum, serverContextId, 0);

        SetServerContext(serverContext2);

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

    public ReceptionCheck CancelCheck(int checkId, DateTime cancelDate)
    {
        var check = _planning.Checks.Find(checkId);

        if (check == default)
        {
            throw new InvalidOperationException("checkId not found");
        }

        _planning.Entry(check).State = EntityState.Detached;

        var entity = _planning.Checks.Update(check with
        {
            Cancelled = true, 
            CancelledDate = cancelDate
        });
        _planning.SaveChanges();
        entity.State = EntityState.Detached;

        return entity.Entity;
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

    public RoomServiceDuty CancelDuty(int dutyId, DateTime cancelDate)
    {
        var duty = _planning.Duties.Find(dutyId);

        if (duty == default)
        {
            throw new InvalidOperationException("dutyId not found");
        }

        _planning.Entry(duty).State = EntityState.Detached;

        var entity = _planning.Duties.Update(duty with
        {
            Cancelled = true, 
            CancelledDate = cancelDate
        });
        _planning.SaveChanges();
        entity.State = EntityState.Detached;

        return entity.Entity;
    }

    public void SetDutyBusyTime(int dutyId, DateTime busyTime, double busyDayNum)
    {
        var duty = _planning.Duties.Find(dutyId);

        if (duty == default)
        {
            throw new InvalidOperationException("dutyId not found");
        }

        _planning.Entry(duty).State = EntityState.Detached;

        var entity = _planning.Duties.Update(duty with { BusyTime = busyTime, BusyDayNum = busyDayNum});
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

        var already = _planning.ServerContext
            .Any(serverContext2 => serverContext2.ServerContextId == serverContext.ServerContextId);

        var entity = _planning.Entry(serverContext);

        entity.State = already
            ? EntityState.Modified
            : EntityState.Added;

        _planning.SaveChanges();

        entity.State = EntityState.Detached;
    }

}