/*
 * ClassHelper
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

using System.Dynamic;
using System.Reflection;
using System.Text;

namespace VinZ.Common.Class;

public static partial class ClassHelper
{
    public static F? GetField<F>(this object? t, string fieldName)
    {
        object? ret;
        if (t is Dynamitey.DynamicObjects.Dictionary o)
        {
            o.TryGetValue(fieldName, out ret);
        } 
        else 
        { 
            ret = t
                ?.GetType()
                ?.GetField(fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(t);
        }

        if (ret == null)
        {
            return default;
        }

        if (ret is string str)
        {
            if (typeof(F) == typeof(int))
            {
                return (F)(object)int.Parse(str);
            } 
            
            if (Nullable.GetUnderlyingType(typeof(F)) == typeof(int))
            {
                if (int.TryParse(str, out var parsed))  
                    return (F)(object)parsed;
            }
            
            if (typeof(F) == typeof(DateTime))
            {
                return (F)(object)DateTime.Parse(str, null, System.Globalization.DateTimeStyles.RoundtripKind);
            } 
            
            if (Nullable.GetUnderlyingType(typeof(F)) == typeof(DateTime))
            {
                if (DateTime.TryParse(str, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                    return (F)(object)parsed;
            }
        }

        return (F)ret;
    }

    public static bool HasField(this object? t, string fieldName)
        => t?.GetType()
            ?.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance)
            != null;


    public static dynamic Combine(dynamic? item1, dynamic? item2)
    {
        var dict1 = (IDictionary<string, object>)(item1 ?? new ExpandoObject());
        var dict2 = (IDictionary<string, object>)(item2 ?? new ExpandoObject());
        var dict12 = dict1.Concat(dict2);

        var ret = new ExpandoObject();
        var dict = ret as IDictionary<string, object>;

        foreach (var kv in dict12)
        {
            dict[kv.Key] = kv.Value;
        }

        //var debugDict1 = JsonConvert.SerializeObject(dict1);
        //var debugDict2 = JsonConvert.SerializeObject(dict2);
        //var debugDict12 = JsonConvert.SerializeObject(dict12);

        return ret;
    }


    public static dynamic? Dynamize(this Dictionary<string, object?> dict)
    {
        var o = new ExpandoObject();
        var pairs = (IDictionary<string, object?>)o;

        foreach (var pair in dict)
        {
            pairs.Add(pair);
        }

        return o;
    }
    public static string ToString(dynamic? item)
    {
        if (item == null)
        {
            return "";
        }
        var dict = (IDictionary<string, object>)item;

        StringBuilder buffer = new ();
        
        foreach (var kv in dict)
        {
            if (buffer.Length > 0)
            {
                buffer.Append(", ");
            }
            buffer.Append($"{kv.Key}={kv.Value}");
        }

        return buffer.ToString ();
    }






    public static Type? ToType(this string? typeName)
    {
        if (typeName == null)
        {
            return null;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var dataType = assembly.GetType(typeName);

            if (dataType != null)
            {
                return dataType;
            }
        }

        return null;
    }





    private static string SignatureNoModifiers(this MethodInfo methodInfo)
    {
        var parameters = methodInfo.GetParameters();

        if (parameters.Length > 0)
        {
            return $"{parameters[0].Member}";
        }
        
        return $"{methodInfo.ReturnType} {methodInfo.Name}()";
    }

    public static string Signature(this MethodInfo methodInfo, bool modifiers = true)
    {
        var sb = new StringBuilder();

        if (modifiers)
        {
            if (methodInfo.IsPrivate)
            {
                sb.Append("private ");
            }
            else if (methodInfo.IsPublic)
            {
                sb.Append("public ");
            }

            if (methodInfo.IsAbstract)
            {
                sb.Append("abstract ");
            }

            if (methodInfo.IsStatic)
            {
                sb.Append("static ");
            }

            if (methodInfo.IsVirtual)
            {
                sb.Append("virtual ");
            }
        }

        sb.Append(SignatureNoModifiers(methodInfo));

        return sb.ToString();
    }
}