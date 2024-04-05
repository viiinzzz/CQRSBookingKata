namespace BookingKata.Infrastructure.Network;

public partial class BillingBus
{
    private const string? Billing = null;

    public static string Recipient = nameof(Billing);

    public static class Verb
    {
        public const string QuotationRequest = $"{nameof(Billing)}:{nameof(QuotationRequest)}";
        public const string QuotationEmitted = $"{nameof(Billing)}:{nameof(QuotationEmitted)}";

        public const string InvoiceRequest = $"{nameof(Billing)}:{nameof(InvoiceRequest)}";
        public const string InvoiceEmitted = $"{nameof(Billing)}:{nameof(InvoiceEmitted)}";

        public const string PaymentRequest = $"{nameof(Billing)}:{nameof(PaymentRequest)}";
        public const string ReceiptEmitted = $"{nameof(Billing)}:{nameof(ReceiptEmitted)}";

        public const string RefundRequest = $"{nameof(Billing)}:{nameof(RefundRequest)}";
        public const string RefundEmitted = $"{nameof(Billing)}:{nameof(RefundEmitted)}";
    }
}