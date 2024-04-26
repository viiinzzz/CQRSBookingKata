namespace Infrastructure.Storage;

public class GazeteerContext : MyDbContext
{
    public DbSet<GeoIndex> Indexes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<GazeteerContext>(IsDebug, IsTrace);
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