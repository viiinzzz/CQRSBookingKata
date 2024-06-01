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

namespace BookingKata.API.Demo;

public partial class DemoBus
{
    private static readonly string[] StepsFakeEmployees = [$"{nameof(DemoBus)}.{nameof(Fake_Employees)}"];

    private void Fake_Employees()
    {
        try
        {
            var originator = GetType().FullName
                             ?? throw new ArgumentException("invalid originator");
            

            int createAndEnrollEmployee(FakeHelper.FakeEmployee fakeEmployee)
            {
                var employeeId = bus.AskResult<Id<Employee>>(
                    Recipient.Admin,Verb.Admin.RequestCreateEmployee,
                    new CreateEmployeeRequest
                    {
                        LastName = fakeEmployee.LastName,
                        FirstName = fakeEmployee.FirstName,
                        SocialSecurityNumber = fakeEmployee.SocialSecurityNumber
                    }, 
                    originator, StepsFakeEmployees);

                if (employeeId == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(employeeId));
                }
                    
                var payrollId = bus.AskResult<Id<PayrollRef>>(Support.Services.Billing.Recipient, Support.Services.Billing.Verb.RequestPayroll,
                    new PayrollRequest
                    {
                        employeeId = employeeId.id,
                        monthlyIncome = fakeEmployee.MonthlyIncome,
                        currency = fakeEmployee.Currency
                    }, 
                    originator, StepsFakeEmployees);

                if (payrollId == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(payrollId));
                }

                return employeeId.id;
            }


            {
                var message = "Demo: Seeding Staff...";

                bus.Notify((ClientNotification)new AdvertiseOptions {
                    MessageText = message,
                    Originator = originator,
                    Immediate = true
                });

                demoContextService.FakeStaffIds = FakeHelper
                    .GenerateFakeEmployees(HotelCount * StaffPerHotel)
                    .AsParallel()
                    .Select(createAndEnrollEmployee)
                    .ToArray();
            }


            {
                var message = "Demo: Seeding Managers...";

                bus.Notify((ClientNotification)new AdvertiseOptions{
                    MessageText = message,
                    Originator = originator,
                    Immediate = true
                });

                demoContextService.FakeManagerIds = FakeHelper
                    .GenerateFakeEmployees(HotelCount * ManagerPerHotel)
                    .Select(createAndEnrollEmployee)
                    .ToArray();
            }
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Employees)}' failed", e);
        }
    }
}