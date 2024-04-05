namespace BookingKata.Infrastructure.Network;

public partial class PlanningBus(IScopeProvider sp) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient);

        Notified += (sender, notification) =>
        {
            using var scope = sp.GetScope<PlanningQueryService>(out var planning);

            var originator = notification.Originator;
            var correlationGuid = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2).Guid;

            try
            {
                switch (notification.Verb)
                {
                    // case Verb.QuotationRequest:
                    //     {
                    //         var request = notification.Json == default
                    //             ? throw new Exception("invalid request")
                    //             : JsonConvert.DeserializeObject<QuotationRequest>(notification.Json);
                    //
                    //         var optionStartUtc = request.optionStartUtc == default
                    //             ? default
                    //             : DateTime.ParseExact(request.optionStartUtc, "s", CultureInfo.InvariantCulture,
                    //                 DateTimeStyles.AssumeUniversal);
                    //         var optionEndUtc = request.optionEndUtc == default
                    //             ? default
                    //             : DateTime.ParseExact(request.optionEndUtc, "s", CultureInfo.InvariantCulture,
                    //                 DateTimeStyles.AssumeUniversal);
                    //
                    //         var jsonMeta = request.jsonMeta == default
                    //             ? "{}"
                    //             : JsonConvert.SerializeObject(JsonConvert.DeserializeObject(request.jsonMeta));
                    //
                    //         //
                    //         //
                    //         var id = billing.EmitQuotation
                    //         (
                    //             request.price,
                    //             request.currency,
                    //             optionStartUtc,
                    //             optionEndUtc,
                    //             jsonMeta,
                    //
                    //             request.referenceId,
                    //             notification.CorrelationId1,
                    //             notification.CorrelationId2
                    //         );
                    //         //
                    //         //
                    //
                    //         Notify(new NotifyMessage(Bus.Recipient.Any, Verb.QuotationEmitted)
                    //         {
                    //             CorrelationGuid = correlationGuid,
                    //             Message = new { id }
                    //         });
                    //     }
                    //     break;


                }
            }
            catch (Exception ex)
            {
                Notify(new NotifyMessage(originator, RequestProcessingError)
                {
                    CorrelationGuid = correlationGuid,
                    Message = new
                    {
                        request = notification.Json,
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        };
    }
}