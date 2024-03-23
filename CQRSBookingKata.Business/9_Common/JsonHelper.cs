namespace CQRSBookingKata.Common;

public static class JsonHelper
{
    private static readonly string BaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

    public static JToken GetJsonAsset(this string nameWithoutExt, string? subFolder = default)
    {
        var assetPath = Path.GetFullPath(Path.Combine(
            subFolder == default ? BaseDir : Path.Combine(BaseDir, subFolder),
            $"{nameWithoutExt}.json"));

        using var file = File.OpenText(assetPath);

        using var reader = new JsonTextReader(file);

        return JToken.ReadFrom(reader);
    }

    public static string[] GetJsonStringArray(this string nameWithoutExt, string? subFolder = default)

        => GetJsonAsset(nameWithoutExt, subFolder)
            .Values<string>()
            .Where(str => str != null)
            .Select(str => $"{str}")
            .ToArray();

    public static TElement?[] GetJsonObjectArray<TElement>(this string nameWithoutExt, string? subFolder = default) 
        where TElement : class
        
        => GetJsonAsset(nameWithoutExt, subFolder)
            ?.Value<JArray>()
            ?.Select(token => token.ToObject<TElement>())
            ?.ToArray() ?? Array.Empty<TElement>();
}