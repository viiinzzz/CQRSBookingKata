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

using System.Reflection;
using System.Text.RegularExpressions;

namespace VinZ.Common;

public static class TypeHelper
{
    public static Type? GetTypeFromFullNameWithLoading(this string fullName, int? argCountRequired = default)
    {
        var type = fullName.GetTypeFromFullName(argCountRequired);

        if (type != null)
        {
            return type;
        }

        var assDefRx = new Regex(@"^([^,]+), ([^,]+)(,.*)*$");

        var m = assDefRx.Match(fullName);

        if (!m.Success)
        {
            return null;
        }

        var typeName = m.Groups[1].Value;
        var assemblyName = m.Groups[2].Value;

        var binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assExt = Path.GetExtension(Assembly.GetExecutingAssembly().Location);
        var assemblyPath = Path.Combine(binDir, assemblyName + assExt);

        var ass = Assembly.LoadFrom(assemblyPath);
        type = ass.GetType(typeName);

        return type;
    }

    public static Type? GetTypeFromFullName(this string fullName, int? argCountRequired = default)
    {
        // var dependencies = Directory.EnumerateFiles(
        //     Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), 
        //     "*" + Path.GetExtension(Assembly.GetExecutingAssembly().Location)
        //     ).ToArray();


        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (a.IsDynamic)
            {
                continue;
            }

            foreach (Type type in a.GetTypes())
            {
                var typeFullName = type.FullName;

                if (typeFullName == null)
                {
                    continue;
                }

                var backQuote = typeFullName.IndexOf('`');

                if (backQuote > 0)
                {
                    if (!int.TryParse(typeFullName[(backQuote+1)..], out var argCount) ||
                        (argCountRequired.HasValue && argCountRequired.Value != argCount))
                    {
                        continue;
                    }

                    typeFullName = typeFullName[..backQuote];
                }

                if (fullName.Equals(typeFullName))
                {
                    return type;
                }
            }
        }

        return null;
    }

    public static IEnumerable<Type> FindGenericInterfaces(this Type type, Type interfaceType)
    {
        return type.GetInterfaces().Where(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == interfaceType);
    }

    public static IEnumerable<Type> EnumerateTypeHierarchy(this Type type)
    {
        var nextType = type;

        while (nextType is not null)
        {
            yield return nextType;

            nextType = nextType.BaseType;
        }
    }

}


