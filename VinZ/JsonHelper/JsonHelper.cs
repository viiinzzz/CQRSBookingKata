using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace VinZ.Common;

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




    public static TEntity Patch<TEntity, TPartEntity>(this TEntity? current, TPartEntity? patch)
        where TEntity : class
        where TPartEntity : class
    {
        if (current == null)
        {
            throw new ArgumentNullException(nameof(current));
        }

        if (patch == null)
        {
            return current;
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

    public static ExpandoObject PatchRelax<TEntity, TEntity2>(this TEntity? current, TEntity2? merge)
        where TEntity : class
        where TEntity2 : class
    {
        var retObj = current == null ? new() :JObject.FromObject(current);

        var mergeObj = merge == null ? new() : JObject.FromObject(merge);

        retObj?.Merge(mergeObj, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });

        var retStr = retObj?.ToString();

        return JsonConvert.DeserializeObject<ExpandoObject>(retStr,
            new ExpandoObjectConverter());
    }

}