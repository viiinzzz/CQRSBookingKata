namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_3_Sales(WebApplication app)
    {
        var sales = app.MapGroup("/sales"
            ).WithOpenApi();

        var customers = sales.MapGroup("/customers"
        ).WithOpenApi();


        customers.MapListMq<Customer>("/", "/sales/customers", filter: null,
            Recipient.Sales, RequestPage, originator,
            responseTimeoutSeconds
        ).WithOpenApi();

    }
}