namespace VinZ.Common;

public static partial class ApiHelper
{
    public static IEnumerable<Type> GetConfigurationTypes(this WebApplicationBuilder? builder, string key,
        IEnumerable<Type> availableTypes)
    {
        if (builder == null)
        {
            return [];
        }

        var typesStr = builder.GetConfigurationValues(key);

        return availableTypes.Where(type => typesStr.Contains(type.FullName));
    }

    public static IEnumerable<(Type typeInterface, Type typeImplementation)> GetConfigurationTypes(
        this WebApplicationBuilder? builder, string key,
        IEnumerable<(Type typeInterface, Type typeImplementation)> availableTypes)
    {
        if (builder == null)
        {
            return [];
        }

        var typesStr = builder.GetConfigurationValues(key);

        return availableTypes.Where(type => typesStr.Contains(type.typeInterface.FullName));
    }

    public static IServiceCollection AddScopedConfigurationTypes(this WebApplicationBuilder? builder, string key,
        IEnumerable<(Type typeInterface, Type typeImplementation)> availableTypes)
    {
        var types = builder.GetConfigurationTypes(key, availableTypes);

        var services = builder.Services;

        foreach (var (serviceType, implementationType) in types)
        {
            if (serviceType == implementationType)
            {
                services.AddScoped(implementationType);
                continue;
            }

            services.AddScoped(serviceType, implementationType);
        }

        return services;
    }

    public static TEnum EnumConfiguration<TEnum>(this WebApplicationBuilder? builder, string key, string defaultKey,
        TEnum defaultValue)
        where TEnum : struct
    {
        if (builder == null)
        {
            return defaultValue;
        }

        if (!Enum.TryParse<TEnum>(builder.Configuration[defaultKey], true, out var configDefaultValue))
        {
            configDefaultValue = defaultValue;
        }

        if (!Enum.TryParse<TEnum>(builder.Configuration[key], true, out var value))
        {
            value = configDefaultValue;
        }

        return value;
    }

    public static Type[] GetConfigurationTypes(this WebApplicationBuilder builder, string? key)
    {
        return builder.Configuration.GetConfigurationTypes(key);
    }

    public static Type[] GetConfigurationTypes(this IConfiguration config, string? key)
    {
        if (key == null)
        {
            return [];
        }

        var typesStr = config.GetConfigurationValues(key);

        return typesStr
            .Select(typeStr => typeStr.GetTypeFromFullNameWithLoading()
                               ?? throw new ApplicationException($"type not found '{typeStr}'"))
            .ToArray();
    }

    public static string[] GetConfigurationValues(this WebApplicationBuilder builder, string? key)
    {
        return builder.Configuration.GetConfigurationValues(key);
    }

    public static string[] GetConfigurationValues(this IConfiguration config, string? key)
    {
        if (key == null)
        {
            return [];
        }

        var values = new List<string>();
        var i = 0;

        while (true)
        {
            var value = config[$"{key}:{i++}"];

            if (value == null)
            {
                break;
            }

            values.Add(value);
        }

        return [.. values];
    }

    private static readonly string[] True = ["1", "true"];

    public static bool IsTrueConfiguration(this WebApplicationBuilder builder, string? key)
    {
        return builder.Configuration.IsTrue(key);
    }

    public static bool IsTrue(this IConfiguration config, string? key)
    {
        if (key == null)
        {
            return false;
        }

        var value = config[key] ?? string.Empty;

        return True.Contains(value.ToLower());
    }

    public static string? GetConfigurationValue(this WebApplicationBuilder builder, string? key)
    {
        return builder.Configuration.GetConfigurationValue(key);
    }

    public static string? GetConfigurationValue(this IConfiguration config, string? key)
    {
        if (key == null)
        {
            return null;
        }

        var value = config[key];

        return value;
    }
}