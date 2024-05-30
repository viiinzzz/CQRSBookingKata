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

    private void Fake_Customers()
    {
        try
        {
            {
                var message = "Demo: Seeding Customers...";

                bus.Notify(new AdvertisementNotification(message)
                {
                    _steps = [nameof(Fake_Customers)],
                    Originator = originator,
                    Immediate = true
                });
            }

            int createCustomer(FakeHelper.FakeCustomer fake)
            {
                var createCustomerRequest = new CreateCustomerRequest
                {
                    EmailAddress = fake.EmailAddress
                };

                var customerId = bus.AskResult<Id<Customer>>(Recipient.Sales, Verb.Sales.RequestCreateCustomer,
                    createCustomerRequest,
                    originator, [nameof(Fake_Customers)]);

                if (customerId == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(customerId));
                }

                demoContextService.FakeCustomersDictionary[customerId.id] = fake;

                return customerId.id;
            }

            demoContextService.FakeCustomerIds = FakeHelper
                .GenerateFakeCustomers(CustomerCount)
                .AsParallel()
                .Select(createCustomer)
                .ToArray();

        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Employees)}' failed", e);
        }
    }
}