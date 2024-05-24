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

//using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace VinZ.Common.Class;

public static class AnonymousHelper
{
    public static bool IsArray(this object obj)
    {
        return obj.GetType().IsArray;
    }

    public static object[] CastToArray(this object arr)
    {
        return (object[])arr;
    }

    public static Dictionary<string, object> AnonymousToDictionary(this object anonymous)
    {
        return anonymous
            .GetType()
            .GetProperties()
            .ToDictionary(
                x => x.Name,
                x => x.GetValue(anonymous, null));
    }

    public static string AnonymousToString(this object anonymous)
    {
        return string.Join(Environment.NewLine, anonymous
            .GetType()
            .GetProperties()
            .Select(x => $"  {x.Name}: {x.GetValue(anonymous, null)}"));
    }

    public static string DictionaryToString(this Dictionary<string, object> dict)
    {
        return string.Join(Environment.NewLine, dict
            .Select(x => $"  {x.Key}: {x.Value}"));
    }


    public static bool IsAnonymous(this Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var d = type.GetGenericTypeDefinition();

        if (!d.IsClass ||
            !d.IsSealed ||
            !d.Attributes.HasFlag(TypeAttributes.NotPublic))
        {
            return false;
        }

        var attributes = d.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);

        if (attributes.Length == 0)
        {
            return false;
        }

        return true;
    }

    public static bool IsAnonymousType<T>(this T obj)
    {
        return IsAnonymous(typeof(T));
    }


    public static bool IsPropertiesHolder(this object obj)
    {
        return obj.GetType().IsClass && obj.GetType().GetProperties().Length > 0;
    }

    public static Dictionary<string, object> PropertiesHolderToDictionary(this object obj)
    {
        return obj.GetType().GetProperties()
            .ToDictionary(
                x => x.Name,
                x => x.GetValue(obj, null));
    }

}