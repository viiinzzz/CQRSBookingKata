
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

services.AddScoped<IAssetsRepository, AssetsRepository>();
services.AddScoped<ISalesRepository, SalesRepository>();
services.AddScoped<IBillingRepository, BillingRepository>();
services.AddScoped<IPlanningRepository, PlanningRepository>();

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

var manager = app.MapGroup("/admin");

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

    dbContextFactory.RegisterDbContextType(() => new MessageQueueContext());

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
