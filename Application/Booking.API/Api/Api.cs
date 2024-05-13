/*╭----------------------------------------------------------------------------╮
  ╎                                                                            ╎
  ╎                             B O O K I N G  API                             ╎
  ╎                                                                            ╎
  ╰----------------------------------------------------------------------------╯*/



/*╭-----------------------------------------------------------------------------
  ╎ basic variables
  */
{
    //for type dependency diagram, establish dependency
    _ = nameof(BookingKata.Infrastructure.EnterpriseStorage);
    _ = nameof(BookingKata.Infrastructure.EnterpriseNetwork);
}


var (isDebug, isRelease, programInfoStr) = ProgramInfo.Get();


var pauseOnError = false; //isDebug;

var demoMode = true; //isDebug;


var url = ApiHelper.GetAppUrl();


var myIps = ApiHelper.GetMyIps();

Console.WriteLine($@"
Network:
{string.Join(Environment.NewLine, myIps.Select(ip => $"http://{ip}:{url.Port}"))}
");


const int DbContextKeepAliveMilliseconds = 30_000;

var dbContextTypes = Types.From
<
    BookingAdminContext,
    BookingSalesContext,
    BookingPlanningContext,

    MessageQueueContext,
    MoneyContext,
    GazeteerContext
>();

var busTypes = Types.From
<
    AdminBus,
    SalesBus,
    PlanningBus,

    BillingBus,
    ThirdPartyBus,
    ConsoleAuditBus
>();
/*
                                                                              ╎
 -----------------------------------------------------------------------------╯*/





/*╭-----------------------------------------------------------------------------
  ╎ DI methods
  */

//adding components to the magic wiring box, aka. DI Container to achieve IoC

void ConfigureDependencyInjection(WebApplicationBuilder builder)
{
    var services = builder.Services;

    //infra
    services.AddSingleton<IScopeProvider, ScopeProvider>();
    services.AddExceptionHandler<GlobalExceptionHandler>();

    //web
    services.AddRazorPages();

    //open api doc
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    //app services
    services.AddSingleton<IServerContextService, ServerContextService>();
    services.AddSingleton<ITimeService, TimeService>();
    services.AddSingleton<IRandomService, RandomService>();

    //security
    services.AddAntiforgery();
    services.AddCors();
    services.AddAuthentication().AddJwtBearer();
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-8.0

    //app bus
    var busConfig = new BusConfiguration
    {
        LocalUrl = url.ToString(),
        RemoteUrl = url.ToString()
    };
    services.AddMessageQueue(busConfig, busTypes, pauseOnError);

    //app repo
    services.AddScoped<IAdminRepository, AdminRepository>();
    services.AddScoped<ISalesRepository, SalesRepository>();
    services.AddScoped<IPlanningRepository, PlanningRepository>();

    services.AddScoped<IMessageQueueRepository, MessageQueueRepository>();
    services.AddScoped<IMoneyRepository, MoneyRepository>();

    //business
    services.AddScoped<AdminQueryService>();
    services.AddScoped<SalesQueryService>();
    services.AddScoped<BookingCommandService>();
    services.AddScoped<PlanningQueryService>();
    services.AddScoped<PlanningCommandService>();
    services.AddScoped<KpiQueryService>();

    //support/third-party
    services.AddScoped<IBillingCommandService, BillingCommandService>();
    services.AddScoped<IGazetteerService, GazetteerService>();
    services.AddScoped<IPaymentCommandService, PaymentCommandService>();
    services.AddScoped<IPricingQueryService, PricingQueryService>();

    var bookingConfig = new BookingConfiguration
    {
        PrecisionMaxKm = 0.5
    };
    services.AddSingleton(bookingConfig);

    //metrics
    services.AddOpenTelemetry().WithMetrics(builder =>
    {
        builder.AddPrometheusExporter();

        builder.AddMeter(
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Server.Kestrel"
            );

        builder.AddView(
            "http-server-request-duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries =
                [
                    0, 0.005, 0.01, 0.025, 0.05,
                    0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10
                ]
            });
    });

    //demo
    services.AddSingleton<BookingDemoContext>();
    if (demoMode)
    {
        services.AddScoped<DemoService>();
        services.AddHostedService<DemoHostService>();
        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
        });
    }

}
/*
                                                                              ╎
 -----------------------------------------------------------------------------╯*/




var builder = WebApplication.CreateSlimBuilder(args);

if (!Enum.TryParse<LogLevel>(builder.Configuration["Logging:LogLevel:Default"], true, out var logLevelDefault))
{
    logLevelDefault = LogLevel.Warning;
}

if (!Enum.TryParse<LogLevel>(builder.Configuration["Logging:LogLevel:Microsoft.EntityFrameworkCore.DbContext"], true, out var logLevelEFContext))
{
    logLevelEFContext = logLevelDefault;
}

builder.RegisterDbContexts(dbContextTypes, isDebug, logLevelEFContext);

ConfigureDependencyInjection(builder);

var api = builder.Build();

var (isDevelopment, isStaging, isProduction, env) = api.GetEnv();

Console.WriteLine($"{programInfoStr} ({env ?? "undefined"})");


api.UseMiddleware<MyDebugMiddleware>();


if (isDevelopment)
{

    api.UseSwagger();
    api.UseSwaggerUI();
    // api.UseExceptionHandler();
}

api.EnsureDatabaseCreated(dbContextTypes, logLevelEFContext, isDebug ? DbContextKeepAliveMilliseconds : null);

api.UseStaticFiles();

MapRoutes(api);

api.MapRazorPages();
api.UseAntiforgery();


api.MapPrometheusScrapingEndpoint(); // path=/metrics



api.Run();

//let the show start...