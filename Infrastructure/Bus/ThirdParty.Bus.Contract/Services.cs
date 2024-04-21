namespace Support.Services;

public static class ThirdParty
{
    public const string Recipient = nameof(ThirdParty);

    public static class Verb
    {
        public const string RequestPricing = $"{Recipient}:{nameof(RequestPricing)}";
        public const string RespondPricing = $"{Recipient}:{nameof(RespondPricing)}";

        public const string RequestPayment = $"{Recipient}:{nameof(RequestPayment)}";
        public const string PaymentAccepted= $"{Recipient}:{nameof(PaymentAccepted)}";
        public const string PaymentRefused= $"{Recipient}:{nameof(PaymentRefused)}";
    }
}