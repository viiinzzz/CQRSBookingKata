using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace VinZ.ToolBox;

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

        => nameWithoutExt.GetJsonAsset(subFolder)
            .Values<string>()
            .Where(str => str != null)
            .Select(str => $"{str}")
            .ToArray();

    public static TElement?[] GetJsonObjectArray<TElement>(this string nameWithoutExt, string? subFolder = default)
        where TElement : class

        => nameWithoutExt.GetJsonAsset(subFolder)
            ?.Value<JArray>()
            ?.Select(token => token.ToObject<TElement>())
            ?.ToArray() ?? Array.Empty<TElement>();




    public static TEntity Patch<TEntity, TPartEntity>(this TEntity current, TPartEntity patch)
        where TEntity : class
        where TPartEntity : class
    {
        if (current == null)
        {
            throw new ArgumentNullException(nameof(current));
        }

        var retObj = JObject.FromObject(current);
        var patchObj = JObject.FromObject(patch);

        retObj?.Merge(patchObj, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });

        return retObj?.ToObject<TEntity>()

               ?? throw new ArgumentNullException(nameof(retObj));
    }

}