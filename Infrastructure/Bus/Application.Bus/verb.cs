namespace Infrastructure.Network.Bus;

public static class Recipient
{
    public const string? Any = default;

    public const string Application = nameof(Application);

    public const string Audit = nameof(Audit);
    public const string Time = nameof(Time);

}

public static class Verb
{
    public const string? Any = default;

    public const string RequestProcessingError = $"{nameof(Recipient.Application)}:{nameof(RequestProcessingError)}";

    public const string Warning = $"{nameof(Recipient.Application)}:{nameof(Warning)}";
    public const string Information = $"{nameof(Recipient.Application)}:{nameof(Information)}";
    public const string Debug = $"{nameof(Recipient.Application)}:{nameof(Debug)}";
}