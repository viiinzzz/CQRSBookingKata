namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string SalesTag = "Sales";

    private static void MapRoutes_3_Sales(WebApplication app)
    {
        var sales = app.MapGroup("/sales"
            ).WithOpenApi().WithTags(new[] { RestrictedTag, AdminTag, SalesTag });

        var customers = sales.MapGroup("/customers"
            ).WithOpenApi().WithTags(new[] { RestrictedTag, AdminTag, SalesTag });

        customers.MapListMq<Customer>("/", "/sales/customers", filter: null,
            Recipient.Sales, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags(new[] { RestrictedTag, AdminTag, SalesTag });

    }
}