
using Newtonsoft.Json;

namespace BookingKata.Planning;

public class PlanningBus(IScopeProvider sp) : MessageBusClientBase
{
    private void WithPlanning(Action<PlanningQueryService> action)
    {
        using var scope = sp.GetScope<PlanningQueryService>(out var planning);

        action(planning);
    }

    public override void Configure()
    {
        // Subscribe(Recipient.Planning);
        Subscribe(Bus.Any, Verb.Sales.NewBooking);

        Notified += (sender, notification) =>
        {
            WithPlanning(planning =>
            {

                switch (notification.Verb)
                {
                    case Verb.Sales.NewBooking:
                        if (notification.Json != default)
                        {
                            // var o = JsonConvert.DeserializeObject<dynamic>(notification.Json)
                            var newBooking = JsonConvert.DeserializeObject<NewBooking>(notification.Json);
                        }
                        break;

                    default:
                        break;
                }
            });
        };
    }
}
