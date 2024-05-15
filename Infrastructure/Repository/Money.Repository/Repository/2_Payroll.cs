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

namespace Support.Infrastructure.Storage;

public partial class MoneyRepository
{

    public IQueryable<Payroll> Payrolls

        => _money.Payrolls
            .AsNoTracking();


    public int EnrollEmployee(int employeeId, double monthlyBaseIncome, string currency)
    {
        var payroll = new Payroll(monthlyBaseIncome, currency, employeeId);

        _money.Payrolls.Add(payroll);
        _money.SaveChanges();
        _money.Entry(payroll).State = EntityState.Detached;

        return payroll.PayrollId;
    }

    public void Deroll(int employeeId)
    {
        var rows = _money.Payrolls
            .Where(payroll => payroll.EmployeeId == employeeId)
            .ExecuteDelete();

        if (rows == 0)
        {
            throw new InvalidOperationException("employeeId has no payroll");
        }
    }
}