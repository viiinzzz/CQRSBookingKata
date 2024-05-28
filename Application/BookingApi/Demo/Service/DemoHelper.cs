namespace BookingKata.API.Demo;

public static class DemoHelper
{
    public static IServiceCollection ConfigureDemo(this IServiceCollection services)
    {
        services.AddSingleton<DemoContextService>();
        services.AddSingleton<IDemoContext>(sp => sp.GetRequiredService<DemoContextService>());

        services.AddSingleton<DemoBus>(sp =>
        {
            var demoContextService = sp.GetRequiredService<DemoContextService>();
            var bus = sp.GetRequiredService<IMessageBus>();
            var scp = sp.GetRequiredService<IScopeProvider>();
            var DateTime = sp.GetRequiredService<ITimeService>();

            var domainBus = new DemoBus(demoContextService, bus, DateTime, scp);

            var client = (IMessageBusClient)domainBus;

            var scope0 = scp.GetScope<MessageQueueConfiguration>(out var mqConfig);
            var scope1 = scp.GetScope<ILogger<IMessageBus>>(out var log);

            var logLevel = mqConfig.logLevel;

            
            if (logLevel <= LogLevel.Trace)
            {
                log.LogInformation(
                    $"Connecting {nameof(DemoBus)} to remote bus {mqConfig.busUrl}...");
            }


            client.Log = log;

            client.ConnectToBus(scp);

            client
                .Configure()
                .ContinueWith(prev =>
                {
                    if (logLevel <= LogLevel.Trace)
                    {
                        log.LogInformation(
                            $"<<<{nameof(DemoBus)}:{client.GetHashCode().xby4()}>>> Connected.");
                    }
                });
             

            return domainBus;
        });

        services.AddHostedService<DemoHostService>();
        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
        });

        return services;
    } 
    
    public static IServiceCollection ConfigureDemoProxy(this IServiceCollection services)
    {
        services.AddSingleton<IDemoContext, DemoContextProxyService>();

        return services;
    }
}