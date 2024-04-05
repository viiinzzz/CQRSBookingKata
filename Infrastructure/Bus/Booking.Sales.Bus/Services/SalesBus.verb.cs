namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private const string? Sales = null;

    public static string Recipient = nameof(Sales);

    public static class Verb
    {
        public const string OpenHotelSeasonRequest = $"{nameof(Sales)}:{nameof(OpenHotelSeasonRequest)}";
        public const string HotelSeasonOpened = $"{nameof(Sales)}:{nameof(HotelSeasonOpened)}";

        public const string BookRequest = $"{nameof(Sales)}:{nameof(BookRequest)}";
        public const string BookConfirmed = $"{nameof(Sales)}:{nameof(BookConfirmed)}";
    }
}