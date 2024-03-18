namespace CQRSBookingKata.API;

public partial class AssetsRepository(IDbContextFactory factory, ITimeService DateTime) : IAssetsRepository
{
    private readonly BookingBackContext _back = factory.CreateDbContext<BookingBackContext>();
}