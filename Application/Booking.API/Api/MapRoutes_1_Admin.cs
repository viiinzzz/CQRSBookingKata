namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string originator = "Api";
    private static void MapRoutes_1_Admin(WebApplication app, out RouteGroupBuilder admin)
    {
        admin = app.MapGroup("/admin"
            ).WithOpenApi();


        admin.MapListMq<Vacancy>("/vacancies", "/admin/vacancies", filter: null, 
            Recipient.Sales, RequestPage, originator,
            responseTimeoutSeconds: responseTimeoutSeconds
            ).WithOpenApi();

        admin.MapListMq<Shared.Booking>("/bookings", "/admin/bookings", filter: null,
            Recipient.Sales, RequestPage, originator, 
            responseTimeoutSeconds: responseTimeoutSeconds
            ).WithOpenApi();

        admin.MapListMq<GeoIndex>("/geo/indexes", "/admin/geo/indexes", filter: null,
            Recipient.Admin, RequestPage, originator,
            responseTimeoutSeconds: responseTimeoutSeconds
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
                Recipient.Sales, Verb.Sales.RequestKpi, originator,
                id,
                requestCancel, responseTimeoutSeconds);


            var html = @$"
<h1>B O O K I N G  API</h1>
<h2>Key Performance Indicators</h2>
<ul>
<li>Occupancy Rate: {kpi.OccupancyRate:P}
</ul>
";

            return Results.Content(html, "text/html");
        }).WithOpenApi();

    }
}