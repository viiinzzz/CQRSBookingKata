using CQRSBookingKata.Billing;
using CQRSBookingKata.Sales;
using Microsoft.EntityFrameworkCore;

namespace CQRSBookingKata.API.Databases;

public class BookingSensitiveContext : DbContext
{
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseSqlite("Data Source=./BookingSensitive.db;");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Invoice>()
            .Property(invoice => invoice.InvoiceId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Payroll>()
            .Property(payroll => payroll.PayrollId)
            .ValueGeneratedOnAdd();
    }
}