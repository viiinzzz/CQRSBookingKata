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

public class MoneyContext : MyDbContext
{
    public DbSet<Quotation> Quotations { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<Refund> Refunds { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        var options = new DbContextHelper.ConfigureMyWayOptions(IsDebug, Env, logLevel);

        builder.ConfigureMyWay<MoneyContext>(options);
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Quotation>()
            .Property(quotation => quotation.QuotationId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Invoice>()
            .Property(invoice => invoice.InvoiceId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Receipt>()
            .Property(receipt => receipt.ReceiptId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Refund>()
            .Property(refund => refund.RefundId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Payroll>()
            .Property(payroll => payroll.PayrollId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Payroll>()
            .HasIndex(payroll => payroll.EmployeeId)
            .IsUnique();

    }
}