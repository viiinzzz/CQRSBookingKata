namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_1_Admin(WebApplication app, out RouteGroupBuilder admin)
    {
        admin = app.MapGroup("/admin"
            ).WithOpenApi();


        admin.MapListMq<Vacancy>("/vacancies", "/admin/vacancies",
            Recipient.Sales, RequestPage, responseTimeoutSeconds
            ).WithOpenApi();

        admin.MapListMq<Shared.Booking>("/bookings", "/admin/bookings",
            Recipient.Sales, RequestPage, responseTimeoutSeconds
            ).WithOpenApi();

        admin.MapListMq<GeoIndex>("/geo/indexes", "/admin/geo/indexes",
            Recipient.Admin, RequestPage, responseTimeoutSeconds
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

    }
}