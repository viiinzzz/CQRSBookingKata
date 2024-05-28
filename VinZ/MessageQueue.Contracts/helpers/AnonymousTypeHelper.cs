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

using System.Dynamic;

namespace VinZ.MessageQueue;

public static class AnonymousTypeHelper
{
    private static readonly Type RuntimeType = typeof(Type).GetType();

    public static bool IsAnonymousType(this Type type)
    {
        if (type == RuntimeType)
        {
            return true;
        }

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
        
        if (attributes is not { Length: > 0 })
        {
            return false;
        }

        return true;
    }

    public static bool IsAnonymous(this object? instance)
    {
        if (instance == null)
        {
            return false;
        }

        if (instance is JObject or ExpandoObject)
        {
            return true;
        }

        var type = instance.GetType();
        
        return IsAnonymousType(type);
    }
}