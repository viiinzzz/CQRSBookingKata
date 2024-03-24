
namespace CQRSBookingKata.API;

public class BookingAdminContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<HotelCell> HotelCells { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<BookingAdminContext>();

        builder.EnableSensitiveDataLogging();
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Employee>()
            .Property(employee => employee.EmployeeId)
            .ValueGeneratedOnAdd();


        builder
            .Entity<Hotel>()
            .Property(hotel => hotel.HotelId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Hotel>()
            .HasIndex(hotel => hotel.HotelName)
            .IsUnique();

        builder
            .Entity<Hotel>()
            .Ignore(hotel => hotel.Position);

        builder
            .Entity<Hotel>()
            .Ignore(hotel => hotel.CellsArray)
            .Ignore(hotel => hotel.Cells12)
;


        builder
            .Entity<HotelCell>()
            .Property(cell => cell.HotelCellId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Hotel>()
            .HasMany(hotel => hotel.Cells);


        builder
            .Entity<Room>()
            .HasKey(room => room.Urid);


        builder
            .Entity<Booking>()
            .Property(booking => booking.BookingId)
            .ValueGeneratedOnAdd();
    }
}