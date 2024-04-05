namespace BookingKata.Infrastructure.Network;

public partial class BillingBus(IScopeProvider sp) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient);

        Notified += (sender, notification) =>
        {
            using var scope = sp.GetScope<BillingCommandService>(out var billing);

            var originator = notification.Originator;
            var correlationGuid = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2).Guid;

            try
            {
                switch (notification.Verb)
                {
                    case Verb.QuotationRequest:
                        {
                            var request = notification.Json == default
                                ? throw new Exception("invalid request")
                                : JsonConvert.DeserializeObject<QuotationRequest>(notification.Json);

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

                            Notify(new NotifyMessage(Infrastructure.Network.Bus.Recipient.Any, Verb.QuotationEmitted)
                            {
                                CorrelationGuid = correlationGuid,
                                Message = new { id }
                            });
                        }
                        break;



                    case Verb.InvoiceRequest:
                        {
                            var request = notification.Json == default
                                ? throw new Exception("invalid request")
                                : JsonConvert.DeserializeObject<InvoiceRequest>(notification.Json);

                            //
                            //
                            var id = billing.EmitInvoice
                            (
                                request.amount,
                                request.currency,

                                request.customerId,
                                request.quotationId,
                                notification.CorrelationId1,
                                notification.CorrelationId2
                            );
                            //
                            //

                            Notify(new NotifyMessage(Bus.Recipient.Any, Verb.QuotationEmitted)
                            {
                                CorrelationGuid = correlationGuid,
                                Message = new { id }
                            });
                        }
                        break;


                    case Verb.PaymentRequest:
                        {
                            var request = notification.Json == default
                                ? throw new Exception("invalid request")
                                : JsonConvert.DeserializeObject<PaymentRequest>(notification.Json);

                            var secret = new DebitCardSecrets(request.ownerName, request.expire, request.CCV);
                            //
                            //
                            var id = billing.EmitReceipt
                            (
                                request.debitCardNumber,
                                secret,

                                request.invoiceId,
                                notification.CorrelationId1,
                                notification.CorrelationId2
                            );
                            //
                            //

                            Notify(new NotifyMessage(Bus.Recipient.Any, Verb.QuotationEmitted)
                            {
                                CorrelationGuid = correlationGuid,
                                Message = new { id }
                            });
                        }
                        break;


                    case Verb.RefundRequest:
                        {
                            var request = notification.Json == default
                                ? throw new Exception("invalid request")
                                : JsonConvert.DeserializeObject<RefundRequest>(notification.Json);
                            //
                            //
                            var id = billing.EmitRefund
                            (
                                request.receiptId,
                                notification.CorrelationId1,
                                notification.CorrelationId2
                            );
                            //
                            //

                            Notify(new NotifyMessage(Bus.Recipient.Any, Verb.QuotationEmitted)
                            {
                                CorrelationGuid = correlationGuid,
                                Message = new { id }
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Notify(new NotifyMessage(originator, Infrastructure.Network.Bus.Verb.RequestProcessingError)
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