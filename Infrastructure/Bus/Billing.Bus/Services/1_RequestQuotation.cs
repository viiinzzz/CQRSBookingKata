namespace Common.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestQuotation(IClientNotification notification, IScopeProvider sp)
    {
        var request = notification.MessageAs<QuotationRequest>();

        var optionStartUtc = request.optionStartUtc == default
            ? default
            : DateTime.ParseExact(request.optionStartUtc, "s", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal);

        var optionEndUtc = request.optionEndUtc == default
            ? default
            : DateTime.ParseExact(request.optionEndUtc, "s", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal);

        var jsonMeta = request.jsonMeta == default
            ? "{}"
            : JsonConvert.SerializeObject(JsonConvert.DeserializeObject(request.jsonMeta));

        using var scope = sp.GetScope<BillingCommandService>(out var billing);

        //
        //
        var id = billing.EmitQuotation
        (
            request.price,
            request.currency,
            optionStartUtc,
            optionEndUtc,
            jsonMeta,
            request.referenceId,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        Notify(new Notification(Omni, QuotationEmitted)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = new { id }
        });
    }
}