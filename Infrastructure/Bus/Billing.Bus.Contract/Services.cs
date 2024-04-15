namespace Common.Services;

public static class Billing
{
    public const string Recipient = nameof(Billing);

    public static class Verb
    {
        public const string RequestQuotation = $"{Recipient}:{nameof(RequestQuotation)}";
        public const string QuotationEmitted = $"{Recipient}:{nameof(QuotationEmitted)}";

        public const string RequestInvoice = $"{Recipient}:{nameof(RequestInvoice)}";
        public const string InvoiceEmitted = $"{Recipient}:{nameof(InvoiceEmitted)}";

        public const string RequestPayment = $"{Recipient}:{nameof(RequestPayment)}";
        public const string ReceiptEmitted = $"{Recipient}:{nameof(ReceiptEmitted)}";

        public const string RequestRefund = $"{Recipient}:{nameof(RequestRefund)}";
        public const string RefundEmitted = $"{Recipient}:{nameof(RefundEmitted)}";
    }
}