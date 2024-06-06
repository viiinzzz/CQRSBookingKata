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

using System.ComponentModel.DataAnnotations.Schema;

namespace BookingKata.Infrastructure.Storage;

public class BookingAdminContext: MyDbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        var options = new DbContextHelper.ConfigureMyWayOptions(IsDebug, Env, logLevel);

        builder.ConfigureMyWay<BookingAdminContext>(options);
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Employee>()
            .HasKey(employee => employee.EmployeeId);

        builder
            .Entity<Hotel>()
            .HasKey(hotel => hotel.HotelId);

        builder
            .Entity<Employee>()
            .HasKey(employee => employee.EmployeeId);


        builder
            .Entity<Employee>()
            .Property(employee => employee.EmployeeId)
            .HasColumnOrder(0)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Hotel>()
            .Property(hotel => hotel.HotelId)
            .HasColumnOrder(0)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Hotel>()
            .HasIndex(hotel => hotel.HotelName)
            .IsUnique();

        builder
            .Entity<Hotel>()
            .Ignore(hotel => hotel.Position)
            .Ignore(hotel => hotel.geoIndex)
            .Ignore(hotel => hotel.Cells);


        builder
            .Entity<Room>()
            .HasKey(room => room.Urid);

    }
}