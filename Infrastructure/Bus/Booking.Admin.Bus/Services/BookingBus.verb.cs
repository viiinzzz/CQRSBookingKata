namespace BookingKata.Infrastructure.Network;

public partial class AdminBus
{
    private const string? Admin = null;

    public static string Recipient = nameof(Admin);

    public static class Verb
    {
        // public const string QuotationRequest = $"{nameof(Admin)}:{nameof(QuotationRequest)}";
        // public const string QuotationEmitted = $"{nameof(Admin)}:{nameof(QuotationEmitted)}";
    }
}