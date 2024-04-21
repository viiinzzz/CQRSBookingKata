using Support.Infrastructure.Network;

namespace Support.Infrastructure.Bus.ThirdParty;

public partial class ThirdPartyBus
{
    private void Verb_Is_RequestPricing(IClientNotification notification)
    {
        var request = notification.MessageAs<PricingRequest>();

        var arrivalDateUtc = request.arrivalDateUtc.DeserializeUniversal_ThrowIfNull(nameof(request.arrivalDateUtc));
        var departureDateUtc = request.departureDateUtc.DeserializeUniversal_ThrowIfNull(nameof(request.departureDateUtc));

        var customerProfile = request.customerProfileJson == null
            ? null
            : JsonConvert.DeserializeObject<CustomerProfile>(request.customerProfileJson);


        using var scope = sp.GetScope<IPricingQueryService>(out var pricing);

        var price = pricing.GetPrice
        (
            request.personMaxCount,
            request.floorNum,
            request.floorNumMax,
            request.hotelRank,
            request.latitude,
            request.longitude,
            request.personCount,
            arrivalDateUtc,
            departureDateUtc,
            request.currency,
            customerProfile
        );

        Notify(new ResponseNotification(Omni, RespondPricing)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = price
        });
    }
}