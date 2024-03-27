/*╭----------------------------------------------------------------------------╮
  ╎                                                                            ╎
  ╎                             B O O K I N G  API                             ╎
  ╎                                                                            ╎
  ╰----------------------------------------------------------------------------╯*/



/*╭-----------------------------------------------------------------------------
  ╎ Storage methods
*/


void EnsureDatabaseCreated<TContext>(WebApplication app) where TContext : DbContext
{
    app
        .Services
        .GetRequiredService<IDbContextFactory>()
        .CreateDbContext<TContext>()
        .Database
        .EnsureCreated();
}

void EnsureAllDatabasesCreated(WebApplication app)
{
    EnsureDatabaseCreated<BookingSalesContext>(app);
    EnsureDatabaseCreated<BookingAdminContext>(app);
    EnsureDatabaseCreated<BookingMoneyContext>(app);
    EnsureDatabaseCreated<BookingPlanningContext>(app);
    EnsureDatabaseCreated<BookingGazeteerContext>(app);
}
/*
                                                                             ╎
-----------------------------------------------------------------------------╯*/



/*╭-----------------------------------------------------------------------------
  ╎ DI methods
*/

void RegisterDbContexts(WebApplicationBuilder webApplicationBuilder)
{
    var dbContextFactory = new RegisteredDbContextFactory();

    dbContextFactory.RegisterDbContextType(() => new MessageQueueContext());

    dbContextFactory.RegisterDbContextType(() => new BookingSalesContext());
    dbContextFactory.RegisterDbContextType(() => new BookingAdminContext());
    dbContextFactory.RegisterDbContextType(() => new BookingMoneyContext());
    dbContextFactory.RegisterDbContextType(() => new BookingPlanningContext());
    dbContextFactory.RegisterDbContextType(() => new BookingGazeteerContext());

    webApplicationBuilder.Services.AddSingleton<IDbContextFactory>(dbContextFactory);
}

void ConfigureDependencyInjection(WebApplicationBuilder builder)
{
    var services = builder.Services;

    //adding components to the magic wiring box, aka. DI Container

    services.AddRazorPages();

    //backend
    services.AddSingleton<MessageQueueServer>();
    services.AddSingleton<IMessageQueueServer>(sp => sp.GetRequiredService<MessageQueueServer>());
    services.AddSingleton<IMessageBus>(sp => sp.GetRequiredService<MessageQueueServer>());
    services.AddHostedService(sp => sp.GetRequiredService<MessageQueueServer>());

    services.AddScoped<IAdminRepository, AdminRepository>();
    services.AddScoped<IMoneyRepository, MoneyRepository>();
    services.AddScoped<IPlanningRepository, PlanningRepository>();
    services.AddScoped<ISalesRepository, SalesRepository>();

    //third-party
    services.AddScoped<IGazetteerService, GazetteerService>();
    services.AddScoped<IPaymentCommandService, PaymentCommandService>();
    services.AddScoped<PricingQueryService>();
    services.AddSingleton<ITimeService, TimeService>();

    // services.AddHttpContextAccessor();
    services.AddAntiforgery();
    
    //business
    services.AddScoped<SalesQueryService>();
    services.AddScoped<BookingCommandService>();
    services.AddScoped<PlanningQueryService>();
    services.AddScoped<PkiQueryService>();

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
        NullableDouble speedFactor,
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
        (int id, [FromServices] PkiQueryService pki) =>
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

    employees.MapPost("/", 
        ([FromBody] NewEmployee spec, [FromServices] IAdminRepository admin)
            => admin.Create(spec));
    employees.MapGet("/", 
        (int? page, int? pageSize, [FromServices] IAdminRepository admin)
            => admin.Employees
                .Page("/admin/employees", page, pageSize));
    employees.MapGet("/{id}",
        (int id, [FromServices] IAdminRepository admin)
            => admin.GetEmployee(id).AsResult());
    employees.MapPatch("/{id}", 
        (int id, [FromBody] UpdateEmployee update, [FromServices] IAdminRepository admin)
            => admin.Update(id, update, scoped: true));
    employees.MapDelete("/{id}", (int id, bool? disable, [FromServices] IAdminRepository admin)
            => admin.DisableEmployee(id, disable ?? true, scoped: true));

    hotels.MapPost("/",
        ([FromBody] NewHotel spec, [FromServices] IAdminRepository admin)
            => admin.Create(spec));
    hotels.MapGet("/",
        (int? page, int? pageSize, [FromServices] IAdminRepository admin, [FromServices] IGazetteerService geo)
            => admin.Hotels
                .Page("/admin/hotels", page, pageSize)
                .IncludeGeoIndex(SalesQueryService.PrecisionMaxKm, geo));
    admin.MapGet("/vacancies",
        (int? page, int? pageSize, [FromServices] ISalesRepository sales, [FromServices] IGazetteerService geo)
            => sales.Vacancies
                .Page("/admin/vacancies", page, pageSize)
                .IncludeGeoIndex(SalesQueryService.PrecisionMaxKm, geo));
    admin.MapGet("/bookings",
        (int? page, int? pageSize, [FromServices] IAdminRepository admin)
            => admin.Bookings
                .Page("/admin/bookings", page, pageSize));
    hotels.MapGet("/{id}", 
        (int id, [FromServices] IAdminRepository admin)
            => admin.GetHotel(id).AsResult());
    hotels.MapPatch("/{id}",
        (int id, [FromBody] UpdateHotel update, [FromServices] IAdminRepository admin)
            => admin.Update(id, update, scoped: true));
    hotels.MapDelete("/{id}", 
        (int id, bool? disable, [FromServices] IAdminRepository admin)
            => admin.DisableHotel(id, disable ?? true, scoped: true));

    admin.MapGet("/geo/indexes",
        (int? page, int? pageSize, [FromServices] IGazetteerService geo)
            => ((GazetteerService)geo).Indexes
                .Page("/admin/geo/indexes", page, pageSize));


    var money = app.MapGroup("/money");

    var payrolls = money.MapGroup("/payrolls");
    payrolls.MapGet("/", 
        (int? page, int? pageSize, [FromServices] IMoneyRepository money2)
            => money2.Payrolls.Page("/money/payrolls", page, pageSize));

    var invoices = money.MapGroup("/invoices");
    invoices.MapGet("/", 
        (int? page, int? pageSize, [FromServices] IMoneyRepository money2)
            => money2.Invoices.Page("/money/invoices", page, pageSize));

    var sales = app.MapGroup("/sales");
    var customers = sales.MapGroup("/customers");
    customers.MapGet("/", 
        (int? page, int? pageSize, [FromServices] ISalesRepository sales2)
            => sales2.Customers.Page("/sales/customers", page, pageSize));



    var reception = app.MapGroup("/reception");

    reception.MapGet("/hotels/{hotelId}", 
        (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
            => planning.GetReceptionPlanning(hotelId)
                .Page($"/reception/{hotelId}", page, pageSize));

    var service = app.MapGroup("/service");

    var room = service.MapGroup("/room");

    room.MapGet("/hotels/{hotelId}",
        (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning) 
            => planning.GetServiceRoomPlanning(hotelId)
                .Page($"/service/room/{hotelId}", page, pageSize));


    app.MapGet("/booking", (
            int? page, int? pageSize,
            [FromQuery(Name = "arrival")] DateTime arrivalDate,
            [FromQuery(Name = "departure")] DateTime departureDate,
            [FromQuery(Name = "persons")] int personCount,
            [FromQuery(Name = "approx")] bool? approximateNameMatch,
            [FromQuery(Name = "hotel")] string? hotelName,
            [FromQuery(Name = "country")] string? countryCode,
            [FromQuery(Name = "city")] string? cityName,
            [FromQuery(Name = "lat")] NullableDouble latitude,
            [FromQuery(Name = "lon")] NullableDouble longitude,
            [FromQuery(Name = "km")] NullableInt maxKm,
            [FromQuery(Name = "pricemin")] NullableInt priceMin,
            [FromQuery(Name = "pricemax")] NullableInt priceMax,
            [FromQuery(Name = "currency")] string? currency,
            [FromServices] SalesQueryService sales) 
        => sales
            .Find(new StayRequest(
                arrivalDate, departureDate, personCount,
                approximateNameMatch, hotelName, countryCode, cityName,
                latitude.Value, longitude.Value, maxKm.Value,
                priceMin.Value, priceMax.Value, currency))
            .Page($"/booking", page, pageSize));

}

/*
                                                                             ╎
-----------------------------------------------------------------------------╯*/
