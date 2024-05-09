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

var isDebug = false;
var isRelease = false;

{
    var pif = ProgramInfo.Current;
    Console.WriteLine(pif.Print());

    isDebug = pif.IsDebug;
    isRelease = !isDebug;
}


var traceStorage = false;//isDebug; //Entity Framework debugging
var traceNetwork = true;//isDebug; //Bus debugging

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
    services.AddMessageQueue(
        new BusConfiguration
        {
            LocalUrl = url.ToString(),
            RemoteUrl = url.ToString(),
            IsTrace = traceNetwork
        },
        busTypes,
        pauseOnError
    );

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

    var bconf = new BookingConfiguration
    {
        PrecisionMaxKm = 0.5
    };
    services.AddSingleton(bconf);

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

    //debug
    services.AddSingleton(new MyDebugMiddlewareConfig
    {
        IsTrace = traceNetwork
    });
}
/*
                                                                              ╎
 -----------------------------------------------------------------------------╯*/




var builder = WebApplication.CreateSlimBuilder(args);

builder.RegisterDbContexts(dbContextTypes, isDebug, traceStorage);

ConfigureDependencyInjection(builder);

var api = builder.Build();

var isDevelopment = api.Environment.IsDevelopment();
var isStaging = api.Environment.IsStaging();
var isProduction = api.Environment.IsProduction();


api.UseMiddleware<MyDebugMiddleware>();


if (isDevelopment)
{

    api.UseSwagger();
    api.UseSwaggerUI();
    // api.UseExceptionHandler();
}

api.EnsureDatabaseCreated(dbContextTypes, isDebug ? DbContextKeepAliveMilliseconds : null);

api.UseStaticFiles();

MapRoutes(api);

api.MapRazorPages();
api.UseAntiforgery();


api.MapPrometheusScrapingEndpoint(); // path=/metrics



api.Run();

//let the show start...