/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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

    public static ExpandoObject? PatchRelax<TEntity, TEntity2>(this TEntity? current, TEntity2? merge)
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

        return retStr.FromJsonToExpando();
    }


    public static object? FromJson(this string? json)
    {
        if (json == null)
        {
            return null;
        }

        return JsonConvert.DeserializeObject(json);
    }



    public static ExpandoObject? FromJsonToExpando(this string? json)
    {
        if (json == null)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
    }


    public static string ToJson(this object? obj, bool indent = false)
    {
        if (obj == null)
        {
            return "";
        }

        var formatting = indent ? Formatting.Indented : Formatting.None;

        return JsonConvert.SerializeObject(obj, formatting);
    }

    public static string ToJsonIgnoring(this object obj, params string[] ignoreProperties)
    {
        return obj.ToJsonIgnoring(false, ignoreProperties);
    }

    public static string ToJsonIgnoring(this object? obj, bool indent, params string[] ignoreProperties)
    {
        if (obj == null)
        {
            return "";
        }

        var settings = new JsonSerializerSettings
        {
            Formatting = indent ? Formatting.Indented : Formatting.None,
            ContractResolver = new IgnorePropertiesResolver(ignoreProperties)
        };

        return JsonConvert.SerializeObject(obj, settings);
    }
}