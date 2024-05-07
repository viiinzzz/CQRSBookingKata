namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string originator = "Api";


    private const string RestrictedTag = "Restricted";
    private const string AdminTag = "Admin";

    private static void MapRoutes_1_Admin(WebApplication app, out RouteGroupBuilder admin)
    {
        admin = app.MapGroup("/admin"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);


        admin.MapListMq<Vacancy>("/vacancies", "/admin/vacancies", filter: null, 
            Recipient.Sales, RequestPage, originator,
            responseTimeoutSeconds: responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

        admin.MapListMq<Shared.Booking>("/bookings", "/admin/bookings", filter: null,
            Recipient.Sales, RequestPage, originator, 
            responseTimeoutSeconds: responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

        admin.MapListMq<GeoIndex>("/geo/indexes", "/admin/geo/indexes", filter: null,
            Support.Services.ThirdParty.Recipient, RequestPage, originator,
            responseTimeoutSeconds: responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);


        admin.MapGet("/hotels/{id}/kpi", async
            (
                int id,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var kpi = await mq.Ask<KeyPerformanceIndicators>(
                originator, Recipient.Sales, Verb.Sales.RequestKpi, id,
                requestCancel, responseTimeoutSeconds);


            var html = @$"
<h1>B O O K I N G  API</h1>
<h2>Key Performance Indicators</h2>
<ul>
<li>Occupancy Rate: {kpi.OccupancyRate:P}
</ul>
";

            return Results.Content(html, "text/html");
        }).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

    }
}