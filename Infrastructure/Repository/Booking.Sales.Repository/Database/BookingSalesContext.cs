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

public class BookingSalesContext : MyDbContext
{
    public DbSet<Vacancy> Vacancies { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<StayProposition> Propositions { get; set; }
    public DbSet<Shared.Booking> Bookings { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<BookingSalesContext>(IsDebug, Env, logLevel);
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Customer>()
            .Property(customer => customer.CustomerId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Customer>()
            .HasIndex(customer => customer.EmailAddress)
            .IsUnique();


        builder
            .Entity<Vacancy>()
            .Ignore(vacancy => vacancy.Position)
            .Ignore(hotel => hotel.geoIndex)
            .Ignore(hotel => hotel.Cells);

        builder
            .Entity<Shared.Booking>()
            .Property(booking => booking.BookingId)
            .ValueGeneratedOnAdd();
    }
}