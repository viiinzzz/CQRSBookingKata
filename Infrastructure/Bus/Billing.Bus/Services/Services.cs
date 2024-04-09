namespace Common.Services;

public static class Recipient
{
    public const string Billing = nameof(Billing);
}


public static class Verb
{
    public static class Billing 
    {
        public const string QuotationRequest = $"{Recipient.Billing}:{nameof(QuotationRequest)}";
        public const string QuotationEmitted = $"{Recipient.Billing}:{nameof(QuotationEmitted)}";

        public const string InvoiceRequest = $"{Recipient.Billing}:{nameof(InvoiceRequest)}";
        public const string InvoiceEmitted = $"{Recipient.Billing}:{nameof(InvoiceEmitted)}";

        public const string PaymentRequest = $"{Recipient.Billing}:{nameof(PaymentRequest)}";
        public const string ReceiptEmitted = $"{Recipient.Billing}:{nameof(ReceiptEmitted)}";

        public const string RefundRequest = $"{Recipient.Billing}:{nameof(RefundRequest)}";
        public const string RefundEmitted = $"{Recipient.Billing}:{nameof(RefundEmitted)}";
    }
}