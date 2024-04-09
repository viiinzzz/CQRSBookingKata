namespace Common.Services;

public static class Recipient
{
    public const string Billing = nameof(Billing);
}


public static class Verb
{
    public static class Billing 
    {
        public const string RequestQuotation = $"{Recipient.Billing}:{nameof(RequestQuotation)}";
        public const string QuotationEmitted = $"{Recipient.Billing}:{nameof(QuotationEmitted)}";

        public const string RequestInvoice = $"{Recipient.Billing}:{nameof(RequestInvoice)}";
        public const string InvoiceEmitted = $"{Recipient.Billing}:{nameof(InvoiceEmitted)}";

        public const string RequestPayment = $"{Recipient.Billing}:{nameof(RequestPayment)}";
        public const string ReceiptEmitted = $"{Recipient.Billing}:{nameof(ReceiptEmitted)}";

        public const string RequestRefund = $"{Recipient.Billing}:{nameof(RequestRefund)}";
        public const string RefundEmitted = $"{Recipient.Billing}:{nameof(RefundEmitted)}";
    }
}