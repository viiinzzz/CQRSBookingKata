namespace VinZ.MessageQueue;

public static class Recipient
{
    public const string? AnyRecipient = default;

    public const string Application = nameof(Application);

    public const string Audit = nameof(Audit);
    public const string Time = nameof(Time);

}

public static class Verb
{
    public const string? AnyVerb = default;

    public const string RequestProcessingError = $"{nameof(Recipient.Application)}:{nameof(RequestProcessingError)}";

    public const string ImportantMessage = $"{nameof(Recipient.Application)}:{nameof(ImportantMessage)}";
    public const string InformationMessage = $"{nameof(Recipient.Application)}:{nameof(InformationMessage)}";
}