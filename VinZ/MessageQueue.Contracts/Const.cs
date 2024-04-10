namespace VinZ.MessageQueue;

public static class Const
{

    public const string? Omni = default;
    public const string? AnyVerb = default;

    public const string ErrorProcessingRequest = $"{nameof(ErrorProcessingRequest)}";


    public const string Request = $"{nameof(Request)}";

    public const string Create = $"{nameof(Create)}";
    public const string Fetch = $"{nameof(Fetch)}";
    public const string Modify = $"{nameof(Modify)}";
    public const string Delete = $"{nameof(Delete)}";

    public const string RequestCreate = $"{Request}{Create}";
    public const string Created = $"{nameof(Created)}";

    public const string RequestFetch = $"{Request}{Fetch}";
    public const string Fetched = $"{nameof(Fetched)}";

    public const string RequestModify = $"{Request}{Modify}";
    public const string Modified = $"{nameof(Modified)}";

    public const string RequestDelete = $"{Request}{Delete}";
    public const string Deleted = $"{nameof(Deleted)}";




    public const string RequestPage = $"{nameof(RequestPage)}";

    public const string Respond = $"{nameof(Respond)}";


    public const string ImportantMessage = $"{nameof(ImportantMessage)}";
    public const string InformationMessage = $"{nameof(InformationMessage)}";
}