﻿namespace Common.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestQuotation(IClientNotification notification)
    {
        var request = notification.MessageAs<QuotationRequest>();

        var optionStartUtc = request.optionStartUtc.DeserializeUniversal_ThrowIfNull(nameof(request.optionStartUtc));
        var optionEndUtc = request.optionEndUtc.DeserializeUniversal_ThrowIfNull(nameof(request.optionEndUtc));

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