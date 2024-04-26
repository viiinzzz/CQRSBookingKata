namespace BookingKata.Infrastructure.Storage;

public class BookingAdminContext: MyDbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<BookingAdminContext>(IsDebug, IsTrace);

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
            .Ignore(hotel => hotel.Position)
            .Ignore(hotel => hotel.geoIndex)
            .Ignore(hotel => hotel.Cells);


        builder
            .Entity<Room>()
            .HasKey(room => room.Urid);



    }
}