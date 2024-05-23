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

public class GazeteerContext : MyDbContext
{
    public DbSet<GeoIndex> Indexes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<GazeteerContext>(IsDebug, Env, logLevel);
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<GeoIndex>()
            .Property(index => index.GeoIndexId)
            .ValueGeneratedOnAdd();


        builder
            .Entity<GeoIndex>()
            .HasKey(index => index.GeoIndexId);

        builder.Entity<GeoIndex>()
            .HasIndex(p => p.RefererHash);
        //            .IsUnique();
        builder.Entity<GeoIndex>()
            .HasIndex(p => p.RefererTypeHash);
        //            .IsUnique();
        builder.Entity<GeoIndex>()
            .HasIndex(p => p.GeoIndexId);
        //            .IsUnique();
    }
}