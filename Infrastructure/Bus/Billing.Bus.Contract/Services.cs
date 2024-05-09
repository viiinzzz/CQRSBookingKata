namespace Support.Services;

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
        public const string PaymentAccepted = $"{Recipient}:{nameof(PaymentAccepted)}";
        public const string PaymentRefused = $"{Recipient}:{nameof(PaymentRefused)}";

        public const string RequestReceipt = $"{Recipient}:{nameof(RequestReceipt)}";
        public const string ReceiptFound = $"{Recipient}:{nameof(ReceiptFound)}";
        public const string ReceiptNotFound = $"{Recipient}:{nameof(ReceiptNotFound)}";

        public const string RequestRefund = $"{Recipient}:{nameof(RequestRefund)}";
        public const string RefundEmitted = $"{Recipient}:{nameof(RefundEmitted)}";

        public const string RequestPayroll = $"{Recipient}:{nameof(RequestPayroll)}";
        public const string PayrollEmitted = $"{Recipient}:{nameof(PayrollEmitted)}";
    }
}