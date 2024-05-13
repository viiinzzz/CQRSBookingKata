namespace VinZ.Common;

public class ScopeProvider(IServiceProvider sp, IConfiguration appConfig, ILogger<ScopeProvider> log) : IScopeProvider
{
    private readonly LogLevel logLevel = GetLogLevel(appConfig);

    private static LogLevel GetLogLevel(IConfiguration appConfig)
    {

        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:Default"], true, out var logLevelDefault))
        {
            logLevelDefault = LogLevel.Warning;
        }

        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:ScopeProvider"], true, out var logLevel))
        {
            logLevel = logLevelDefault;
        }

        return logLevel;
    }

    public IServiceScope GetScope<T>(out T t) where T : notnull
    {
        var scope = sp.CreateScope();

        t = scope.ServiceProvider.GetRequiredService<T>();

        if (logLevel <= LogLevel.Debug)
        {
            log.LogWarning(@$"

                                              ...GetScope {typeof(T).FullName}...
");
        }

        return scope;
    }

    public IServiceScope GetScope(Type serviceType, out object t)
    {
        var scope = sp.CreateScope();

        t = scope.ServiceProvider.GetRequiredService(serviceType);

        if (logLevel <= LogLevel.Debug)
        {
            log.LogWarning(@$"

                                              ...GetScope {serviceType.FullName}...
");
        }

        return scope;
    }

}