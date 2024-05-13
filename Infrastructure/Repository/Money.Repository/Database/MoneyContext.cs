namespace Infrastructure.Storage;

public class MoneyContext : MyDbContext
{
    public DbSet<Quotation> Quotations { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<Refund> Refunds { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<MoneyContext>(IsDebug, logLevel);
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