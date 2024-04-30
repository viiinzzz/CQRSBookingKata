/*╭----------------------------------------------------------------------------╮
  ╎                                                                            ╎
  ╎                             B O O K I N G  API                             ╎
  ╎                                                                            ╎
  ╰----------------------------------------------------------------------------╯*/

{
    //for type dependency diagram, establish dependency
    _ = nameof(BookingKata.Infrastructure.EnterpriseStorage);
    _ = nameof(BookingKata.Infrastructure.EnterpriseNetwork);
}

var isDebug = false;

{
    var pif = ProgramInfo.Current;
    pif.Print();

    isDebug = pif.IsDebug;
}

var isRelease = !isDebug;


var traceEF = false;//isDebug;
var pauseOnError = false; //isDebug;
var demoMode = true; //isDebug;

/*╭-----------------------------------------------------------------------------
  ╎ Storage methods
  */

void EnsureDatabaseCreated<TContext>(WebApplication app) where TContext : DbContext
{
    var database = app
        .Services
        .GetRequiredService<IDbContextFactory>()
        .CreateDbContext<TContext>()
        .Database;

    var created = database.EnsureCreated();

//     Console.WriteLine(@$"
// {typeof(TContext).Name}: {(created ? "Created" : "Not created")}. {database.GetConnectionString()}
// ");


    if (isDebug)
    {
        //for in-memory sqlite, as currently configured for the 'Debug' build configuration,
        //at least one client connection needs to be kept open, otherwise the database vanishes
        var dbContext = Activator.CreateInstance(typeof(TContext)) as TContext;
        
        var keepAlive = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(30000);

                dbContext.Database.OpenConnection();

//                 Console.WriteLine(@$"
// {typeof(TContext).Name}: Keep Alive. {database.GetConnectionString()}
// ");
            }
        });
    }
}

void EnsureAllDatabasesCreated(WebApplication app)
{
    EnsureDatabaseCreated<BookingAdminContext>(app);
    EnsureDatabaseCreated<BookingSalesContext>(app);
    EnsureDatabaseCreated<BookingPlanningContext>(app);

    EnsureDatabaseCreated<MessageQueueContext>(app);
    EnsureDatabaseCreated<MoneyContext>(app);
    EnsureDatabaseCreated<GazeteerContext>(app);
}

void RegisterDbContext<TContext>(RegisteredDbContextFactory dbContextFactory, bool isDebug) where TContext : MyDbContext, new()
{
    dbContextFactory.RegisterDbContextType(() =>
    {
        var dbContext = new TContext
        {
            IsDebug = isDebug,
            IsTrace = traceEF
        };

        return dbContext;
    });
}

void RegisterDbContexts(WebApplicationBuilder webApplicationBuilder)
{
    var dbContextFactory = new RegisteredDbContextFactory();
    
    RegisterDbContext<BookingAdminContext>(dbContextFactory, isDebug);
    RegisterDbContext<BookingSalesContext>(dbContextFactory, isDebug);
    RegisterDbContext<BookingPlanningContext>(dbContextFactory, isDebug);

    RegisterDbContext<MessageQueueContext>(dbContextFactory, isDebug);
    RegisterDbContext<MoneyContext>(dbContextFactory, isDebug);
    RegisterDbContext<GazeteerContext>(dbContextFactory, isDebug);

    webApplicationBuilder.Services.AddSingleton<IDbContextFactory>(dbContextFactory);
}

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
    services.AddExceptionHandler<GlobalExceptionHandler>();
    services.AddRazorPages();
    services.AddSingleton<IScopeProvider, ScopeProvider>();

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    services.AddSingleton<IServerContextService, ServerContextService>();
    services.AddSingleton<ITimeService, TimeService>();
    services.AddSingleton<IRandomService, RandomService>();
    services.AddAntiforgery();
    services.AddCors();
    services.AddAuthentication().AddJwtBearer();
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-8.0


    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS").Split(";");
    var url = $"{urls.First()}/bus/";

    //bus
    services.AddMessageQueue(
        new BusConfiguration
        {
            LocalUrl = url,
            RemoteUrl = url
        },
        Types.From<AdminBus, SalesBus, PlanningBus, BillingBus, ThirdPartyBus, ConsoleAuditBus>(),
        pauseOnError
    );

    //repo
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

RegisterDbContexts(builder);
ConfigureDependencyInjection(builder);

var api = builder.Build();

var isDevelopment = api.Environment.IsDevelopment();
var isStaging = api.Environment.IsStaging();
var isProduction = api.Environment.IsProduction();


if (isDevelopment)
{
    api.UseSwagger();
    api.UseSwaggerUI();
    // api.UseExceptionHandler();
}

EnsureAllDatabasesCreated(api);

api.UseStaticFiles();
api.UseMiddleware<OperationCanceledMiddleware>();

MapRoutes(api);

api.MapRazorPages();
api.UseAntiforgery();

api.Run();
