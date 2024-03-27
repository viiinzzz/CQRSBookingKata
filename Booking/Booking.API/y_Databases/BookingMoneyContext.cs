
namespace BookingKata.API;

public class BookingMoneyContext : DbContext
{
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)

        => builder.ConfigureMyWay<BookingMoneyContext>();


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

        builder
            .Entity<Payroll>()
            .HasIndex(payroll => payroll.EmployeeId)
            .IsUnique();

    }
}