namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestKpi(IClientNotification notification)
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

        Notify(new Notification(notification.Originator, Verb.Sales.RespondKpi)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = indicators
        });
    }
}