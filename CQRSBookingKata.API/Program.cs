
/*------------------------------------------------------------------------------
 Booking API
*/

var builder = WebApplication.CreateSlimBuilder(args);

RegisterDbContexts(builder);

builder.Services.AddScoped<IAssetsRepository, AssetsRepository>();
builder.Services.AddScoped<ISalesRepository, SalesRepository>();
builder.Services.AddScoped<IBillingRepository, BillingRepository>();

builder.Services.AddScoped<PaymentCommandService>();
builder.Services.AddScoped<PricingQueryService>();
builder.Services.AddSingleton<TimeService>();

builder.Services.AddScoped<SalesQueryService>();
builder.Services.AddScoped<RoomCommandService>();
builder.Services.AddScoped<BookingCommandService>();
builder.Services.AddScoped<PlanningQueryService>();

var app = builder.Build();


EnsureDatabasesCreated(app);


var root = app.MapGet("/", () => new {
    links = new []
    {
        new { group = "Hotel Administration", url = "/manager" },
        new { group = "Reception Planning", url = "/reception" },
        new { group = "RoomService Planning", url = "/service/room" },
        new { group = "Booking", url = "/booking" }
    }
}.AsResult());

var manager = app.MapGroup("/manager");

var employees = manager.MapGroup("/employees");
var hotels = manager.MapGroup("/hotels");

employees.MapPost("/", ([FromBody] NewEmployee spec, [FromServices] IAssetsRepository assets) 
    => assets.Create(spec));
employees.MapGet("/", (int? page, int? pageSize, [FromServices] IAssetsRepository assets) 
    => assets.Employees.Page("/admin/employees", page, pageSize));
employees.MapGet("/{id}", (int id, [FromServices] IAssetsRepository assets) 
    => assets.GetEmployee(id).AsResult());
employees.MapPatch("/{id}", (int id, [FromBody] UpdateEmployee update, [FromServices] IAssetsRepository assets) 
    => assets.Update(id, update));
employees.MapDelete("/{id}", (int id, bool? disable, [FromServices] IAssetsRepository assets)
    => assets.DisableEmployee(id, disable ?? true));

hotels.MapPost("/", ([FromBody] NewHotel spec, [FromServices] IAssetsRepository assets)
    => assets.Create(spec));
hotels.MapGet("/", (int? page, int? pageSize, [FromServices] IAssetsRepository assets) 
    => assets.Hotels.Page("/admin/hotels", page, pageSize));
hotels.MapGet("/{id}", (int id, [FromServices] IAssetsRepository assets) 
    => assets.GetHotel(id).AsResult());
hotels.MapPatch("/{id}", (int id, [FromBody] UpdateHotel update, [FromServices] IAssetsRepository assets) 
    => assets.Update(id, update));
hotels.MapDelete("/{id}", (int id, bool? disable, [FromServices] IAssetsRepository assets) 
    => assets.DisableHotel(id, disable ?? true));

var reception = app.MapGroup("/reception");

reception.MapGet("/{hotelId}", (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
    => planning.GetReceptionPlanning(hotelId).Page($"/reception/{hotelId}", page, pageSize));

var service = app.MapGroup("/service");
var room = service.MapGroup("/room");

room.MapGet("/{hotelId}", (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
    => planning.GetServiceRoomPlanning(hotelId).Page($"/service/room/{hotelId}", page, pageSize));


app.Run();

return;


/*------------------------------------------------------------------------------
 Database routines
*/

void RegisterDbContexts(WebApplicationBuilder webApplicationBuilder)
{
    var dbContextFactory = new RegisteredDbContextFactory();

    dbContextFactory.RegisterDbContextType(() => new BookingFrontContext());
    dbContextFactory.RegisterDbContextType(() => new BookingBackContext());
    dbContextFactory.RegisterDbContextType(() => new BookingSensitiveContext());

    webApplicationBuilder.Services.AddSingleton<IDbContextFactory>(dbContextFactory);
}

void EnsureDatabasesCreated(WebApplication webApplication)
{
    webApplication.Services.GetRequiredService<IDbContextFactory>().CreateDbContext<BookingFrontContext>().Database
        .EnsureCreated();
    webApplication.Services.GetRequiredService<IDbContextFactory>().CreateDbContext<BookingBackContext>().Database
        .EnsureCreated();
    webApplication.Services.GetRequiredService<IDbContextFactory>().CreateDbContext<BookingSensitiveContext>().Database
        .EnsureCreated();
}
