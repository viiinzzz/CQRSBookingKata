
/*------------------------------------------------------------------------------
 Booking API
*/



var builder = WebApplication.CreateSlimBuilder(args);

RegisterDbContexts(builder);

var services = builder.Services;

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
services.AddScoped<PaymentCommandService>();
services.AddScoped<PricingQueryService>();
services.AddSingleton<ITimeService, TimeService>();

//business
services.AddScoped<SalesQueryService>();
services.AddScoped<BookingCommandService>();
services.AddScoped<PlanningQueryService>();


services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = true;
});
services.AddHostedService<DemoService>();

var app = builder.Build();


EnsureDatabasesCreated(app);


var root = app.MapGet("/", () => new {
    links = new []
    {
        new { group = "Hotel Administration", url = "/admin" },
        new { group = "Reception Planning", url = "/reception" },
        new { group = "RoomService Planning", url = "/service/room" },
        new { group = "Booking", url = "/booking" }
    }
}.AsResult());

var admin = app.MapGroup("/admin");

var employees = admin.MapGroup("/employees");
var hotels = admin.MapGroup("/hotels");

employees.MapPost("/", ([FromBody] NewEmployee spec, [FromServices] IAdminRepository assets) 
    => assets.Create(spec));
employees.MapGet("/", (int? page, int? pageSize, [FromServices] IAdminRepository assets) 
    => assets.Employees.Page("/admin/employees", page, pageSize));
employees.MapGet("/{id}", (int id, [FromServices] IAdminRepository assets) 
    => assets.GetEmployee(id).AsResult());
employees.MapPatch("/{id}", (int id, [FromBody] UpdateEmployee update, [FromServices] IAdminRepository assets) 
    => assets.Update(id, update, scoped: true));
employees.MapDelete("/{id}", (int id, bool? disable, [FromServices] IAdminRepository assets)
    => assets.DisableEmployee(id, disable ?? true, scoped: true));

hotels.MapPost("/", ([FromBody] NewHotel spec, [FromServices] IAdminRepository assets)
    => assets.Create(spec));
hotels.MapGet("/", (int? page, int? pageSize, [FromServices] IAdminRepository assets) 
    => assets.Hotels.Page("/admin/hotels", page, pageSize));
hotels.MapGet("/{id}", (int id, [FromServices] IAdminRepository assets) 
    => assets.GetHotel(id).AsResult());
hotels.MapPatch("/{id}", (int id, [FromBody] UpdateHotel update, [FromServices] IAdminRepository assets) 
    => assets.Update(id, update, scoped: true));
hotels.MapDelete("/{id}", (int id, bool? disable, [FromServices] IAdminRepository assets) 
    => assets.DisableHotel(id, disable ?? true, scoped: true));


var reception = app.MapGroup("/reception");

reception.MapGet("/{hotelId}", (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
    => planning.GetReceptionPlanning(hotelId).Page($"/reception/{hotelId}", page, pageSize));

var service = app.MapGroup("/service");
var room = service.MapGroup("/room");

room.MapGet("/{hotelId}", (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
    => planning.GetServiceRoomPlanning(hotelId).Page($"/service/room/{hotelId}", page, pageSize));


var booking = app.MapGroup("/booking");

booking.MapGet("/", (
        [FromQuery(Name = "arrival")] DateTime arrivalDate,
        [FromQuery(Name = "departure")] DateTime departureDate,
        [FromQuery(Name = "person")] int personCount,

        [FromQuery(Name = "approx")] bool? approximateNameMatch,
        [FromQuery(Name = "hotel")] string? hotelName, 
        [FromQuery(Name = "country")] string? countryCode,
        [FromQuery(Name = "city")] string? cityName,

        [FromQuery(Name = "lat")] double? latitude, 
        [FromQuery(Name = "lon")] double? longitude, 
        [FromQuery(Name = "km")] int? maxKm,

        [FromQuery(Name = "pricemin")] double? priceMin, 
        [FromQuery(Name = "pricemax")] double? priceMax, 
        [FromQuery(Name = "currency")] string? currency, 
        int? page, int? pageSize, 
        [FromServices] SalesQueryService sales)

    => sales
        .Find(new StayRequest(
            arrivalDate, departureDate, personCount,
            approximateNameMatch, hotelName, countryCode,cityName,
            latitude, longitude, maxKm, 
            priceMin, priceMax, currency
            ))
        .Page($"/booking", page, pageSize));

app.Run();

return;


/*------------------------------------------------------------------------------
 Database routines
*/

void RegisterDbContexts(WebApplicationBuilder webApplicationBuilder)
{
    var dbContextFactory = new RegisteredDbContextFactory();

    dbContextFactory.RegisterDbContextType(() => new MessageQueueContext());

    dbContextFactory.RegisterDbContextType(() => new BookingSalesContext());
    dbContextFactory.RegisterDbContextType(() => new BookingAdminContext());
    dbContextFactory.RegisterDbContextType(() => new BookingMoneyContext());

    webApplicationBuilder.Services.AddSingleton<IDbContextFactory>(dbContextFactory);
}

void EnsureDatabasesCreated(WebApplication webApplication)
{
    webApplication.Services.GetRequiredService<IDbContextFactory>().CreateDbContext<BookingSalesContext>().Database
        .EnsureCreated();
    webApplication.Services.GetRequiredService<IDbContextFactory>().CreateDbContext<BookingAdminContext>().Database
        .EnsureCreated();
    webApplication.Services.GetRequiredService<IDbContextFactory>().CreateDbContext<BookingMoneyContext>().Database
        .EnsureCreated();
}
