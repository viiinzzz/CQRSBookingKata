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

namespace BookingKata.Sales;

public interface ISalesRepository
{
    IQueryable<Customer> Customers { get; }
    int CreateCustomer(string emailAddress);
    Customer? GetCustomer(int customerId);
    int? FindCustomer(string emailAddress);
    void UpdateCustomer(Customer customer);
    void DisableCustomer(int customerId, bool disable);


    IQueryable<Vacancy> Vacancies { get; }
    void AddVacancies(IEnumerable<Vacancy> newVacancies);
    void RemoveVacancies(IEnumerable<long> vacancyIds);


    IQueryable<StayProposition> Propositions { get; }
    void AddStayProposition(StayProposition proposition);
    bool HasActiveProposition(DateTime now, int urid, DateTime arrival, DateTime departure);


    IQueryable<Shared.Booking> Bookings { get; }
    int AddBooking(Shared.Booking booking);

    Shared.Booking CancelBooking(int bookingId);
}