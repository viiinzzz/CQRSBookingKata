namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string MoneyTag = "Money";

    private static void MapRoutes_2_Money(WebApplication app)
    {
        var money = app.MapGroup("/money"
            ).WithOpenApi().WithTags(new[] { RestrictedTag, AdminTag });

        var payrolls = money.MapGroup("/payrolls"
            ).WithOpenApi().WithTags(new[] { RestrictedTag, AdminTag, MoneyTag });

        var invoices = money.MapGroup("/invoices"
            ).WithOpenApi().WithTags(new[] { RestrictedTag, AdminTag, MoneyTag });


        payrolls.MapListMq<Payroll>("/", "/money/payrolls", filter: null,
            Recipient.Admin, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags(new[] { RestrictedTag, AdminTag, MoneyTag });

        invoices.MapListMq<Invoice>("/", "/money/invoices", filter: null,
            Recipient.Admin, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags(new[] { RestrictedTag, AdminTag, MoneyTag });

    }
}