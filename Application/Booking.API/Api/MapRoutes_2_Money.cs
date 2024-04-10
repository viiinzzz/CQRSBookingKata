namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_2_Money(WebApplication app)
    {
        var money = app.MapGroup("/money"
            ).WithOpenApi();

        var payrolls = money.MapGroup("/payrolls"
            ).WithOpenApi();

        var invoices = money.MapGroup("/invoices"
            ).WithOpenApi();


        payrolls.MapListMq<Payroll>("/", "/money/payrolls",
            Recipient.Admin, RequestPage, responseTimeoutSeconds
        ).WithOpenApi();

        invoices.MapListMq<Invoice>("/", "/money/invoices",
            Recipient.Admin, RequestPage, responseTimeoutSeconds
        ).WithOpenApi();

    }
}