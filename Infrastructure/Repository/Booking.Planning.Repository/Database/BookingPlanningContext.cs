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

public class BookingPlanningContext : MyDbContext
{
    public DbSet<ReceptionCheck> Checks { get; set; }
    public DbSet<RoomServiceDuty> Duties { get; set; }
    public DbSet<ServerContext> ServerContext { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<BookingPlanningContext>(IsDebug, logLevel);
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {

        builder
            .Entity<ReceptionCheck>()
            .HasKey(check => check.CheckId);


        builder
            .Entity<RoomServiceDuty>()
            .HasKey(duty => duty.DutyId);

        // builder
        //     .Entity<RoomServiceDuty>()
        //     .HasOne<ServerContext>(duty => duty.Server);


        builder
            .Entity<ServerContext>()
            .HasKey(serverContext => serverContext.ServerContextId);
    }
}