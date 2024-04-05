/*╭----------------------------------------------------------------------------╮
  ╎                                                                            ╎
  ╎                             B O O K I N G  API                             ╎
  ╎                                                                            ╎
  ╰----------------------------------------------------------------------------╯*/

_ = nameof(BookingKata.Infrastructure.EnterpriseStorage);
_ = nameof(BookingKata.Infrastructure.EnterpriseNetwork);

var pif = ProgramInfo.Current;
pif.Print();


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


    if (pif.IsDebug)
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

void RegisterDbContexts(WebApplicationBuilder webApplicationBuilder)
{
    var dbContextFactory = new RegisteredDbContextFactory();

    dbContextFactory.RegisterDbContextType(() => new BookingAdminContext());
    dbContextFactory.RegisterDbContextType(() => new BookingSalesContext());
    dbContextFactory.RegisterDbContextType(() => new BookingPlanningContext());

    dbContextFactory.RegisterDbContextType(() => new MessageQueueContext());
    dbContextFactory.RegisterDbContextType(() => new MoneyContext());
    dbContextFactory.RegisterDbContextType(() => new GazeteerContext());

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
    services.AddRazorPages();
    services.AddSingleton<IScopeProvider, ScopeProvider>();
    var serverContext = new ServerContextService();
    services.AddSingleton<IServerContextService>(sp => serverContext);
    Console.WriteLine($"{nameof(ServerContext)}: Id={serverContext.Id}");
    services.AddSingleton<ITimeService, TimeService>();
    services.AddSingleton<IRandomService, RandomService>();
    services.AddAntiforgery();

    //bus
    services.AddSingleton<PlanningBus>();
    services.AddSingleton<ConsoleAuditBus>();
    services.AddSingleton<MessageQueueServerConfig>(_ => new ()
    {
        DomainBusType = TypeHelper.Types<PlanningBus, ConsoleAuditBus>()
    });
    services.AddSingleton<MessageQueueServer>();
    services.AddSingleton<IMessageBus>(sp => sp.GetRequiredService<MessageQueueServer>());
    services.AddHostedService(sp => sp.GetRequiredService<MessageQueueServer>());

    //repo
    services.AddScoped<IAdminRepository, AdminRepository>();
    services.AddScoped<ISalesRepository, SalesRepository>();
    services.AddScoped<IPlanningRepository, PlanningRepository>();

    services.AddScoped<IMessageQueueRepository, MessageQueueRepository>();
    services.AddScoped<IMoneyRepository, MoneyRepository>();

    //third-party
    services.AddScoped<IGazetteerService, GazetteerService>();
    services.AddScoped<IPaymentCommandService, PaymentCommandService>();
    services.AddScoped<PricingQueryService>();

    //business
    services.AddScoped<SalesQueryService>();
    services.AddScoped<BookingCommandService>();
    services.AddScoped<PlanningQueryService>();
    services.AddScoped<KpiQueryService>();

    //demo
    services.AddSingleton<BookingDemoContext>();
    services.AddScoped<DemoService>();
    services.AddHostedService<DemoHostService>();
    services.Configure<HostOptions>(options =>
    {
        options.ServicesStartConcurrently = true;
        options.ServicesStopConcurrently = true;
    });
}
/*
                                                                             ╎
-----------------------------------------------------------------------------╯*/



var builder = WebApplication.CreateSlimBuilder(args);

RegisterDbContexts(builder);
ConfigureDependencyInjection(builder);


var api = builder.Build();

// if (api.Environment.IsDevelopment())
// {
    // api.UseSwagger();
    // api.UseSwaggerUI();
    // api.UseExceptionHandler();
// }

api.UseStaticFiles();

EnsureAllDatabasesCreated(api);
MapRoutes(api);

api.MapRazorPages();
api.UseAntiforgery();

api.Run();
return;



/*╭-----------------------------------------------------------------------------
  ╎ Routes
*/

void MapRoutes(WebApplication app)
{

    var demo = app.MapGroup("/demo");

    demo.MapGet("/forward",
        async (
        int days,
        ParsableNullableDouble speedFactor,
        [FromServices] DemoService demos) => 
        {
            try
            {
                var newTime = await demos.Forward(days, speedFactor.Value, CancellationToken.None);

                return Results.Redirect("/");
            }
            catch (Exception ex)
            {
                return Results.Problem(new ProblemDetails() { Title = ex.Message, Detail = ex.StackTrace, Status = 500 });
            }
        });


    var admin = app.MapGroup("/admin");

    admin.MapGet("/hotels/{id}/kpi", 
        (int id, [FromServices] KpiQueryService pki) =>
    {
        var html= @$"
<h1>B O O K I N G  API</h1>
<h2>Key Performance Metrics</h2>
<ul>
<li>Occupancy Rate: {pki.GetOccupancyRate(id):P}
</ul>
";
        return Results.Content(html, "text/html");
    });

    var employees = admin.MapGroup("/employees");
    var hotels = admin.MapGroup("/hotels");

    void QueryCheck<T>(IQueryable<T> query) where T : class
    {
        if (query.LostInTranslation(out var sql, out var translationError))
        {
            throw new Exception($"We apologize, we got lost in translation: {translationError}");
        }
    }

    employees.MapPost("/", 
        ([FromBody] NewEmployee spec, [FromServices] IAdminRepository admin)
            => admin.Create(spec));
    employees.MapGet("/", 
        (int? page, int? pageSize, [FromServices] IAdminRepository admin)
            => admin.Employees
                .Page("/admin/employees", page, pageSize, QueryCheck));
    employees.MapGet("/{id}",
        (int id, [FromServices] IAdminRepository admin)
            => admin.GetEmployee(id).AsResult());
    employees.MapPatch("/{id}", 
        (int id, [FromBody] UpdateEmployee update, [FromServices] IAdminRepository admin)
            => admin.Update(id, update));
    employees.MapDelete("/{id}", (int id, bool? disable, [FromServices] IAdminRepository admin)
            => admin.DisableEmployee(id, disable ?? true));

    hotels.MapPost("/",
        ([FromBody] NewHotel spec, [FromServices] IAdminRepository admin)
            => admin.Create(spec));
    hotels.MapGet("/",
        (int? page, int? pageSize, [FromServices] IAdminRepository admin, [FromServices] IGazetteerService geo)
            => admin.Hotels
                .Page("/admin/hotels", page, pageSize, QueryCheck)
                .IncludeGeoIndex(PrecisionMaxKm, geo));
    admin.MapGet("/vacancies",
        (int? page, int? pageSize, [FromServices] ISalesRepository sales, [FromServices] IGazetteerService geo)
            => sales.Vacancies
                .Page("/admin/vacancies", page, pageSize, QueryCheck)
                .IncludeGeoIndex(PrecisionMaxKm, geo));
    admin.MapGet("/bookings",
        (int? page, int? pageSize, [FromServices] ISalesRepository sales)
            => sales.Bookings
                .Page("/bookings", page, pageSize, QueryCheck));
    hotels.MapGet("/{id}", 
        (int id, [FromServices] IAdminRepository admin)
            => admin.GetHotel(id).AsResult());
    hotels.MapPatch("/{id}",
        (int id, [FromBody] UpdateHotel update, [FromServices] IAdminRepository admin)
            => admin.Update(id, update));
    hotels.MapDelete("/{id}", 
        (int id, bool? disable, [FromServices] IAdminRepository admin)
            => admin.DisableHotel(id, disable ?? true));

    admin.MapGet("/geo/indexes",
        (int? page, int? pageSize, [FromServices] IGazetteerService geo)
            => ((GazetteerService)geo).Indexes
                .Page("/admin/geo/indexes", page, pageSize, QueryCheck));


    var money = app.MapGroup("/money");

    var payrolls = money.MapGroup("/payrolls");
    payrolls.MapGet("/", 
        (int? page, int? pageSize, [FromServices] IMoneyRepository money2)
            => money2.Payrolls.Page("/money/payrolls", page, pageSize, QueryCheck));

    var invoices = money.MapGroup("/invoices");
    invoices.MapGet("/", 
        (int? page, int? pageSize, [FromServices] IMoneyRepository money2)
            => money2.Invoices.Page("/money/invoices", page, pageSize, QueryCheck));

    var sales = app.MapGroup("/sales");
    var customers = sales.MapGroup("/customers");
    customers.MapGet("/", 
        (int? page, int? pageSize, [FromServices] ISalesRepository sales2)
            => sales2.Customers.Page("/sales/customers", page, pageSize, QueryCheck));



    var reception = app.MapGroup("/reception");

    reception.MapGet("/planning/full/hotels/{hotelId}",
        (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
            => planning.GetReceptionFullPlanning(hotelId)
                .Page($"/reception/{hotelId}", page, pageSize, QueryCheck));

    reception.MapGet("/planning/today/hotels/{hotelId}",
        (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
            => planning.GetReceptionTodayPlanning(hotelId)
                .Page($"/reception/{hotelId}", page, pageSize, QueryCheck));

    reception.MapGet("/planning/week/hotels/{hotelId}",
        (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
            => planning.GetReceptionWeekPlanning(hotelId)
                .Page($"/reception/{hotelId}", page, pageSize, QueryCheck));

    reception.MapGet("/planning/month/hotels/{hotelId}",
        (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
            => planning.GetReceptionMonthPlanning(hotelId)
                .Page($"/reception/{hotelId}", page, pageSize, QueryCheck));

    var service = app.MapGroup("/service");

    var room = service.MapGroup("/room");

    room.MapGet("/hotels/{hotelId}",
        (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning) 
            => planning.GetServiceRoomPlanning(hotelId)
                .Page($"/service/room/{hotelId}", page, pageSize, QueryCheck));


    app.MapGet("/booking", (
            int? page, int? pageSize,
            [FromQuery(Name = "arrival")] DateTime arrivalDate,
            [FromQuery(Name = "departure")] DateTime departureDate,
            [FromQuery(Name = "persons")] int personCount,
            [FromQuery(Name = "approx")] bool? approximateNameMatch,
            [FromQuery(Name = "hotel")] string? hotelName,
            [FromQuery(Name = "country")] string? countryCode,
            [FromQuery(Name = "city")] string? cityName,
            [FromQuery(Name = "lat")] ParsableNullableDouble latitude,
            [FromQuery(Name = "lon")] ParsableNullableDouble longitude,
            [FromQuery(Name = "km")] ParsableNullableInt maxKm,
            [FromQuery(Name = "pricemin")] ParsableNullableInt priceMin,
            [FromQuery(Name = "pricemax")] ParsableNullableInt priceMax,
            [FromQuery(Name = "currency")] string? currency,
            [FromServices] SalesQueryService sales) 
        => sales
            .Find(new StayRequest(
                arrivalDate, departureDate, personCount,
                approximateNameMatch, hotelName, countryCode, cityName,
                latitude.Value, longitude.Value, maxKm.Value,
                priceMin.Value, priceMax.Value, currency))
            .Page($"/booking", page, pageSize, QueryCheck));

}

/*
                                                                             ╎
-----------------------------------------------------------------------------╯*/
