using System.Text.RegularExpressions;

namespace VinZ.MessageQueue;

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
}