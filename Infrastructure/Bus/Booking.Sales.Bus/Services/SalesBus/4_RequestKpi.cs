namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestKpi(IClientNotificationSerialized notification)
    {
        var id = notification.MessageAs<int>();

        using var scope = sp.GetScope<KpiQueryService>(out var kpi);

        //
        //
        var indicators = new KeyPerformanceIndicators
        {
            OccupancyRate = kpi.GetOccupancyRate(id),
        };
        //
        //

        Notify(new ResponseNotification(notification.Originator, Verb.Sales.RespondKpi, indicators)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}