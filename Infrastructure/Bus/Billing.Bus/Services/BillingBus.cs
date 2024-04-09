using BookingKata.Infrastructure.Common;
using VinZ.Common;

namespace Common.Infrastructure.Bus.Billing;

public class BillingBus(IScopeProvider sp) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient.Billing);

        Notified += (sender, notification) =>
        {
            using var scope = sp.GetScope<BillingCommandService>(out var billing);
            using var scope2 = sp.GetScope<IMoneyRepository>(out var moneyRepository);

            var originator = notification.Originator;
            var correlationGuid = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2).Guid;

            try
            {
                switch (notification.Verb)
                {
                    case Verb.Billing.QuotationRequest:
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

                        Notify(new NotifyMessage(Omni, QuotationEmitted)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = new { id }
                        });

                        break;
                    }

                    case Verb.Billing.InvoiceRequest:
                    {
                        var request = notification.MessageAs<InvoiceRequest>();

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

                        Notify(new NotifyMessage(Omni, QuotationEmitted)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = new { id }
                        });

                        break;
                    }

                    case Verb.Billing.PaymentRequest:
                    {
                        var request = notification.MessageAs<PaymentRequest>();

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

                        Notify(new NotifyMessage(Omni, QuotationEmitted)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = new { id }
                        });

                        break;
                    }

                    case Verb.Billing.RefundRequest:
                    {
                        var request = notification.MessageAs<RefundRequest>();

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

                        Notify(new NotifyMessage(Omni, QuotationEmitted)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = new { id }
                        });

                        break;
                    }

                    case RequestPage:
                    {
                        var request = notification.MessageAs<PageRequest>();

                        object? page;

                        switch (request.Path)
                        {
                            case "/money/payrolls":
                            {
                                page = moneyRepository
                                    .Payrolls
                                    .Page(request.Path, request.Page, request.PageSize);

                                break;
                            }

                            case "/money/invoices":
                            {
                                page = moneyRepository
                                    .Invoices
                                    .Page(request.Path, request.Page, request.PageSize);

                                break;
                            }

                            default:
                            {
                                throw new NotImplementedException($"page request for path not supported: {request.Path}");
                            }
                        }

                        Notify(new NotifyMessage(originator, RespondPage)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = page
                        });

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Notify(new NotifyMessage(originator, ErrorProcessingRequest)
                {
                    CorrelationGuid = correlationGuid,
                    Message = new
                    {
                        message = notification.Message,
                        messageType = notification.MessageType,
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        };
    }
}