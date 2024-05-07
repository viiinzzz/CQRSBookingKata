namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string MoneyTag = "Money";

    private static void MapRoutes_2_Money(WebApplication app)
    {
        var money = app.MapGroup("/money"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

        var payrolls = money.MapGroup("/payrolls"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, MoneyTag]);

        var invoices = money.MapGroup("/invoices"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, MoneyTag]);


        payrolls.MapListMq<Payroll>("/", "/money/payrolls", filter: null,
            Support.Services.Billing.Recipient, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, MoneyTag]);

        invoices.MapListMq<Invoice>("/", "/money/invoices", filter: null,
            Support.Services.Billing.Recipient, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, MoneyTag]);

    }
}