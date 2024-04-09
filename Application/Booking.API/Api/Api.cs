/*╭----------------------------------------------------------------------------╮
  ╎                                                                            ╎
  ╎                             B O O K I N G  API                             ╎
  ╎                                                                            ╎
  ╰----------------------------------------------------------------------------╯*/

using BookingKata.Shared;

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
    services.AddCors();
    services.AddAuthentication().AddJwtBearer();
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-8.0

    //bus
    services.AddSingleton<PlanningBus>();
    services.AddSingleton<ConsoleAuditBus>();
    services.AddSingleton(_ => new MessageQueueServerConfig
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

    var bconf = new BookingConfiguration
    {
        PrecisionMaxKm = 0.5
    };
    services.AddSingleton(bconf);

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

var isDevelopment = api.Environment.IsDevelopment();
var isStaging = api.Environment.IsStaging();
var isProduction = api.Environment.IsProduction();


if (isDevelopment)
{
    api.UseSwagger();
    api.UseSwaggerUI();
    api.UseExceptionHandler();
}

EnsureAllDatabasesCreated(api);

api.UseStaticFiles();
api.UseMiddleware<OperationCanceledMiddleware>();

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
    const int responseTimeoutSeconds = 120;

    {
        var demo = app.MapGroup("/demo");

        demo.MapGet("/forward", async (
            int days,
            ParsableNullableDouble speedFactor, 
            [FromServices] DemoService demos
            ) =>
        {
            var forward = async () =>
            {
                var newTime = await demos.Forward(days, speedFactor.Value, CancellationToken.None);

                return Results.Redirect("/");
            };

            return await forward.WithStackTrace();

        }).WithOpenApi();
    }



    {
        var admin = app.MapGroup("/admin")
            .WithOpenApi();

        admin.MapGet("/vacancies", async
            (
                int? page, int? pageSize,
                [FromServices] IMessageBus mq, CancellationToken requestCancel
            )
            => await mq.Ask
            (
                Recipient.Sales, RequestPage,
                new PageRequest("/admin/vacancies", page, pageSize),
                requestCancel, responseTimeoutSeconds
            )
        ).WithOpenApi();

        admin.MapGet("/bookings", async
            (
                int? page, int? pageSize,
                [FromServices] IMessageBus mq, CancellationToken requestCancel
            )
            => await mq.Ask
            (
                Recipient.Sales, RequestPage,
                new PageRequest("/admin/bookings", page, pageSize),
                requestCancel, responseTimeoutSeconds
            )
        ).WithOpenApi();


        admin.MapGet("/geo/indexes", async
            (
                int? page, int? pageSize,
                [FromServices] IMessageBus mq, CancellationToken requestCancel
            )
            => await mq.Ask
            (
                Recipient.Admin, RequestPage,
                new PageRequest("/admin/geo/indexes", page, pageSize),
                requestCancel, responseTimeoutSeconds
            )
        ).WithOpenApi();

        admin.MapGet("/hotels/{id}/kpi", async
            (
                int id,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var kpi = await mq.Ask<KeyPerformanceIndicators>
            (
                Recipient.Sales, Verb.Sales.RequestKpi, id,
                requestCancel, responseTimeoutSeconds
            );


            var html = @$"
<h1>B O O K I N G  API</h1>
<h2>Key Performance Indicators</h2>
<ul>
<li>Occupancy Rate: {kpi.OccupancyRate:P}
</ul>
";

            return Results.Content(html, "text/html");
        }).WithOpenApi();



        {
            var employees = admin.MapGroup("/employees")
                .WithOpenApi();

            employees.MapPost("/", async
                (
                    [FromBody] NewEmployee spec,
                    [FromServices] IMessageBus mq,
                    CancellationToken requestCancel
                ) 
                => await mq.Ask
                (
                    Recipient.Admin, Verb.Admin.RequestCreation, 
                    spec, requestCancel, responseTimeoutSeconds
                )
            ).WithOpenApi();

            employees.MapGet("/", async
                (
                    int? page, int? pageSize,
                    [FromServices] IMessageBus mq, CancellationToken requestCancel
                )
                => await mq.Ask
                (
                    Recipient.Admin, RequestPage,
                    new PageRequest("/admin/employees", page, pageSize),
                    requestCancel, responseTimeoutSeconds
                )
             ).WithOpenApi();

            employees.MapGet("/{id}",
                (int id, [FromServices] IAdminRepository admin)
                    => admin.GetEmployee(id).AsResult())
                .WithOpenApi();

            employees.MapPatch("/{id}",
                (int id, [FromBody] UpdateEmployee update, [FromServices] IAdminRepository admin)
                    => admin.Update(id, update))
                .WithOpenApi();

            employees.MapDelete("/{id}", (int id, bool? disable, [FromServices] IAdminRepository admin)
                => admin.DisableEmployee(id, disable ?? true))
                .WithOpenApi();
        }



        {
            var hotels = admin.MapGroup("/hotels")
                .WithOpenApi();

            hotels.MapPost("/",
                ([FromBody] NewHotel spec, [FromServices] IAdminRepository admin)
                    => admin.Create(spec))
                .WithOpenApi();

            hotels.MapGet("/", async
                (
                    int? page, int? pageSize,
                    [FromServices] IMessageBus mq, CancellationToken requestCancel
                )
                => await mq.Ask(
                    Recipient.Admin, RequestPage,
                    new PageRequest("/admin/hotels", page, pageSize),
                    requestCancel, responseTimeoutSeconds
                )
            ).WithOpenApi();

            hotels.MapGet("/{id}",
                (int id, [FromServices] IAdminRepository admin)
                    => admin.GetHotel(id).AsResult())
                .WithOpenApi();

            hotels.MapPatch("/{id}",
                (int id, [FromBody] UpdateHotel update, [FromServices] IAdminRepository admin)
                    => admin.Update(id, update))
                .WithOpenApi();

            hotels.MapDelete("/{id}",
                (int id, bool? disable, [FromServices] IAdminRepository admin)
                    => admin.DisableHotel(id, disable ?? true))
                .WithOpenApi();
        }
    }



    {
        var money = app.MapGroup("/money")
            .WithOpenApi();

        var payrolls = money.MapGroup("/payrolls")
            .WithOpenApi();

        var invoices = money.MapGroup("/invoices")
            .WithOpenApi();

        payrolls.MapGet("/", async
            (
                int? page, int? pageSize,
                [FromServices] IMessageBus mq, CancellationToken requestCancel
            )
            => await mq.Ask(
                Recipient.Admin, RequestPage,
                new PageRequest("/money/payrolls", page, pageSize),
                requestCancel, responseTimeoutSeconds
            )
        ).WithOpenApi();

        invoices.MapGet("/", async
            (
                int? page, int? pageSize,
                [FromServices] IMessageBus mq, CancellationToken requestCancel
            )
            => await mq.Ask(
                Recipient.Admin, RequestPage,
                new PageRequest("/money/invoices", page, pageSize),
                requestCancel, responseTimeoutSeconds
            )
        ).WithOpenApi();
    }



    {
        var sales = app.MapGroup("/sales")
            .WithOpenApi();

        var customers = sales.MapGroup("/customers")
            .WithOpenApi();

        customers.MapGet("/",
            (int? page, int? pageSize, [FromServices] ISalesRepository sales2)
                => sales2.Customers.Page("/sales/customers", page, pageSize))
            .WithOpenApi();
    }



    {
        var reception = app.MapGroup("/reception")
            .WithOpenApi();

        reception.MapGet("/planning/full/hotels/{hotelId}",
            (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
                => planning.GetReceptionFullPlanning(hotelId)
                    .Page($"/reception/{hotelId}", page, pageSize))
            .WithOpenApi();

        reception.MapGet("/planning/today/hotels/{hotelId}",
            (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
                => planning.GetReceptionTodayPlanning(hotelId)
                    .Page($"/reception/{hotelId}", page, pageSize))
            .WithOpenApi();

        reception.MapGet("/planning/week/hotels/{hotelId}",
            (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
                => planning.GetReceptionWeekPlanning(hotelId)
                    .Page($"/reception/{hotelId}", page, pageSize))
            .WithOpenApi();

        reception.MapGet("/planning/month/hotels/{hotelId}",
            (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
                => planning.GetReceptionMonthPlanning(hotelId)
                    .Page($"/reception/{hotelId}", page, pageSize))
            .WithOpenApi();
    }



    {
        var service = app.MapGroup("/service")
            .WithOpenApi();

        var room = service.MapGroup("/room")
            .WithOpenApi();

        room.MapGet("/hotels/{hotelId}",
            (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
                => planning.GetServiceRoomPlanning(hotelId)
                    .Page($"/service/room/{hotelId}", page, pageSize))
            .WithOpenApi();
    }



    {
        //TODO
        var todocustomerId = 0;

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
                [FromServices] SalesQueryService sales
            )
            => sales
                .Find(new StayRequest(
                    arrivalDate, departureDate, personCount,
                    approximateNameMatch, hotelName, countryCode, cityName,
                    latitude.Value, longitude.Value, maxKm.Value,
                    priceMin.Value, priceMax.Value, currency), todocustomerId)
                .Page($"/booking", page, pageSize))
            .WithOpenApi();
    }
}

/*
                                                                             ╎
-----------------------------------------------------------------------------╯*/
