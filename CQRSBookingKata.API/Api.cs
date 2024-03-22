/*╭----------------------------------------------------------------------------╮
  ╎                                                                            ╎
  ╎                             B O O K I N G  API                             ╎
  ╎                                                                            ╎
  ╰----------------------------------------------------------------------------╯*/



/*╭-----------------------------------------------------------------------------
  ╎ Storage methods
*/

using System.Collections.Immutable;
using System.Reflection.Metadata;

void RegisterDbContexts(WebApplicationBuilder webApplicationBuilder)
{
    var dbContextFactory = new RegisteredDbContextFactory();

    dbContextFactory.RegisterDbContextType(() => new MessageQueueContext());

    dbContextFactory.RegisterDbContextType(() => new BookingSalesContext());
    dbContextFactory.RegisterDbContextType(() => new BookingAdminContext());
    dbContextFactory.RegisterDbContextType(() => new BookingMoneyContext());
    dbContextFactory.RegisterDbContextType(() => new BookingPlanningContext());

    webApplicationBuilder.Services.AddSingleton<IDbContextFactory>(dbContextFactory);
}

void EnsureDatabaseCreated<TContext>(WebApplication app) where TContext : DbContext
{
    app.Services.GetRequiredService<IDbContextFactory>().CreateDbContext<TContext>().Database
        .EnsureCreated();
}

void EnsureAllDatabasesCreated(WebApplication app)
{
    EnsureDatabaseCreated<BookingSalesContext>(app);
    EnsureDatabaseCreated<BookingAdminContext>(app);
    EnsureDatabaseCreated<BookingMoneyContext>(app);
    EnsureDatabaseCreated<BookingPlanningContext>(app);
}
/*
                                                                             ╎
-----------------------------------------------------------------------------╯*/



/*╭-----------------------------------------------------------------------------
  ╎ DI methods
*/

void ConfigureDependencyInjection(WebApplicationBuilder builder)
{
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
}
/*
                                                                             ╎
-----------------------------------------------------------------------------╯*/



var builder = WebApplication.CreateSlimBuilder(args);

RegisterDbContexts(builder);

ConfigureDependencyInjection(builder);


var app = builder.Build();

EnsureAllDatabasesCreated(app);

SetupRoutes(app);

app.Run();

return;



/*╭-----------------------------------------------------------------------------
  ╎ Routes
*/

void SetupRoutes(WebApplication app1)
{
    var root = app1.MapGet("/", () =>
    {
        var links = new[]
        {
            // new { group = "Hotel Administration", url = "/admin" },
            new { group = "List of Employees", url = "/admin/employees" },
            new { group = "List of Hotels", url = "/admin/hotels" },
            new { group = "List of Vacancies", url = "/admin/vacancies" },
            new { group = "Reception Planning", url = "/reception" },
            new { group = "Room Service Planning", url = "/service/room" },
            // new { group = "Find a Stay", url = "/booking" }
        };

        var html = $@"
<h1>B O O K I N G  API</h1>
<ul>
{string.Join(Environment.NewLine, links.Select(link => @$"
<li><a href={link.url}>{link.group}</a>"))}

<li>Find a Stay
<form action=""/booking"">

<table  width=""100%""><tbody>

<tr><td>
<label for=""farrival"">Arrival Date</label><br>
<input type=""date"" id=""farrival"" name=""arrival"" value=""{DateTime.Now:yyyy-MM-dd}"" required>
</td><td>
<label for=""fdeparture"">Departure Date</label><br>
<input type=""date"" id=""fdeparture"" name=""departure"" value=""{DateTime.Now.AddDays(1):yyyy-MM-dd}"" required>
</td><td>
<label for=""fpersons"">Persons</label><br>
<input type=""number"" id=""fpersons"" name=""persons"" min=""1"" max=""9"" value=""1""  minlength=""1"" maxlength=""1"" size=""4"" required>
</td></tr>

<tr><td>
<label for=""fcountry"">Country Code</label>/<label for=""fcity"">City Name</label><br>
<input type=""text"" id=""fcountry"" name=""country"" minlength=""2"" maxlength=""2"" size=""2"" required value=""FR"" >&nbsp;
<input type=""text"" id=""fcity"" name=""city"" value=""Paris"">
</td><td>
<input type=""checkbox"" id=""fapprox"" name=""approx"">
<label for=""fapprox"">Approximate Names</label>
</td><td>
<label for=""fhotel"">Hotel Name</label><br>
<input type=""text"" id=""fhotel"" name=""hotel"">
</td></tr>

<tr><td colspan=""2"">
<label for=""flat"">Latitude</label>/<label for=""flon"">Longitude</label>&nbsp;
<input type=""text"" id=""flat"" name=""lat"" size=""10"">/
<input type=""text"" id=""flon"" name=""lon"" size=""10"">
</td><td>
</td></tr>

<tr><td colspan=""2"">
Price/Currency<br>
<label for=""fpricemax"">Maximum</label>&nbsp;
<input type=""text"" id=""fpricemaxvalue"" name=""pricemax"" value=""1000"">&nbsp;
<input type=""text"" id=""fcurrency"" name=""currency""  minlength=""3"" maxlength=""3"" size=""3"" value=""EUR""><br>
<input type=""range"" style=""width:100%;"" id=""fpricemax""  min=""0"" max=""1000"" step=""50"" oninput=""document.getElementById('fpricemaxvalue').value = this.value"" value=""1000"">
</td><td>
</td></tr>

<tr><td colspan=""2"">
<label for=""fpricemin"">Minimum</label>&nbsp;<input type=""text"" id=""fpriceminvalue"" name=""pricemin"" value=""0""><br>
<input type=""range"" style=""width:100%;"" id=""fpricemin""  min=""0"" max=""1000"" step=""50"" oninput=""document.getElementById('fpriceminvalue').value = this.value"" value=""0""><br>
</td><td>
<label for=""fkm"">Kilometers allowance</label><input type=""text"" id=""fkmvalue"" name=""km"" size=""4"" value=""20""><br>
<input type=""range"" style=""width:100%;"" id=""fkm""  min=""0"" max=""50"" step=""5"" oninput=""document.getElementById('fkmvalue').value = this.value"" value=""20"">
</td></tr>

<tr><td colspan=""2"">
</td><td>
<input type=""reset"" value=""Reset"">
</td><td>
<input type=""submit"" value=""Submit"">
</td></tr>

</form>

</ul>
";

        return Results.Content(html, "text/html");
    });

    var admin = app1.MapGroup("/admin");

    admin.MapGet("/vacancies", ([FromServices] ISalesRepository sales) =>
    {
        var ret =  sales.Vacancies
            .ToArray();

        return ret;
    });

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


    var reception = app1.MapGroup("/reception");

    reception.MapGet("/{hotelId}", (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
        => planning.GetReceptionPlanning(hotelId).Page($"/reception/{hotelId}", page, pageSize));

    var service = app1.MapGroup("/service");
    var room = service.MapGroup("/room");

    room.MapGet("/{hotelId}", (int hotelId, int? page, int? pageSize, [FromServices] PlanningQueryService planning)
        => planning.GetServiceRoomPlanning(hotelId).Page($"/service/room/{hotelId}", page, pageSize));


    var booking = app1.MapGroup("/booking");

    booking.MapGet("/", (
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
            int? page, int? pageSize,
            [FromServices] SalesQueryService sales)
        => sales
            .Find(new StayRequest(
                arrivalDate, departureDate, personCount,
                approximateNameMatch, hotelName, countryCode, cityName,
                latitude.Value, longitude.Value, maxKm.Value,
                priceMin.Value, priceMax.Value, currency
            ))
            .Page($"/booking", page, pageSize));

}

/*
                                                                             ╎
-----------------------------------------------------------------------------╯*/


//
//
// public class DateRange : IParsable<DateRange>
// {
//     public DateOnly? From { get; init; }
//     public DateOnly? To { get; init; }
//
//     public static DateRange Parse(string value, IFormatProvider? provider)
//     {
//         if (!TryParse(value, provider, out var result))
//         {
//             throw new ArgumentException("Could not parse supplied value.", nameof(value));
//         }
//
//         return result;
//     }
//
//     public static bool TryParse(string? value,
//         IFormatProvider? provider, out DateRange dateRange)
//     {
//         var segments = value?.Split(',', StringSplitOptions.RemoveEmptyEntries
//                                          | StringSplitOptions.TrimEntries);
//
//         if (segments?.Length == 2
//             && DateOnly.TryParse(segments[0], provider, out var fromDate)
//             && DateOnly.TryParse(segments[1], provider, out var toDate))
//         {
//             dateRange = new DateRange { From = fromDate, To = toDate };
//             return true;
//         }
//
//         dateRange = new DateRange { From = default, To = default };
//         return false;
//     }
// }
