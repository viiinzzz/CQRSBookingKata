namespace BookingKata.API.Demo;

public static class DemoHelper
{
    public static IServiceCollection ConfigureDemo(this IServiceCollection services)
    {
        services.AddSingleton<DemoContextService>();
        services.AddSingleton<IDemoContext>(sp => sp.GetRequiredService<DemoContextService>());

        services.AddSingleton<DemoBus>();

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