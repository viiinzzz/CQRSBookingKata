
namespace BookingKata.Sales;

public class SalesBus(IScopeProvider sp, IMessageBus bus) : MessageBusClientBase
{
    private void WithPlanning(Action<PlanningQueryService> action)
    {
        using var scope = sp.GetScope<PlanningQueryService>(out var planning);

        action(planning);
    }

    public override void Configure()
    {
        ConnectTo(bus);

        // Subscribe(Recipient.Planning);
        // Subscribe(Bus.Any, Verb.Sales.NewBooking);
        //
        // Notified += (sender, notification) =>
        // {
        //     WithPlanning(planning =>
        //     {
        //
        //         switch (notification.Verb)
        //         {
        //             case Verb.Sales.NewBooking:
        //                 var m = notification.Message;
        //                 break;
        //
        //             default:
        //                 break;
        //         }
        //     });
        // };
    }
}