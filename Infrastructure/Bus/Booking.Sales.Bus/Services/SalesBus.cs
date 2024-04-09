namespace BookingKata.Infrastructure.Bus.Sales;

public class SalesBus(IScopeProvider sp) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient.Sales);

        Notified += (sender, notification) =>
        {
            using var scope = sp.GetScope<BookingCommandService>(out var booking);
            using var scope2 = sp.GetScope<KpiQueryService>(out var kpi);
            using var scope3 = sp.GetScope<ISalesRepository>(out var salesRepository);
            using var scope4 = sp.GetScope<IGazetteerService>(out var geo);

            var originator = notification.Originator;
            var correlationGuid = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2).Guid;

            try
            {
                switch (notification.Verb)
                {
                    case Verb.Sales.RequestOpenHotelSeason:
                    {
                        var request = notification.MessageAs<OpenHotelSeasonRequest>();

                        var openingDate = DateTime.ParseExact(request.openingDate, 
                            "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                        var closingDate = DateTime.ParseExact(request.closingDate, 
                            "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                        //
                        //
                        booking.OpenHotelSeason(
                            request.hotelId,
                            request.exceptRoomNumbers,

                            openingDate, 
                            closingDate
                            //
                            // notification.CorrelationId1,
                            // notification.CorrelationId2
                        );
                        //
                        //

                        var opening = new
                        {
                            request.hotelId,
                            openingDate,
                            closingDate
                        };

                        Notify(new NotifyMessage(Omni, Verb.Sales.HotelSeasonOpening)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = opening
                        });

                        break;
                    }

                    case Verb.Sales.RequestBook:
                    {
                        var request = notification.MessageAs<BookRequest>();

                        var debitCardSecrets = new DebitCardSecrets(request.debitCardOwner, request.debitCardExpire, request.debitCardCCV);
                        //
                        //
                        var id = booking.Book
                        (
                            request.lastName,
                            request.firstName,

                            request.debitCardNumber,
                            debitCardSecrets,

                            request.customerId,
                            request.stayPropositionId,

                            notification.CorrelationId1,
                            notification.CorrelationId2
                        );
                        //
                        //

                        Notify(new NotifyMessage(Omni, Verb.Sales.BookConfirmed)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = new
                            {
                                id
                            }
                        });

                        break;
                    }

                    case Verb.Sales.RequestKpi:
                    {
                        var id = notification.MessageAs<int>();

                        //
                        //
                        var indicators = new KeyPerformanceIndicators
                        {
                            OccupancyRate = kpi.GetOccupancyRate(id),
                        };
                        //
                        //

                        Notify(new NotifyMessage(originator, Verb.Sales.RespondKpi)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = indicators
                        });

                        break;
                    }

                    case RequestPage:
                    {
                        var request = notification.MessageAs<PageRequest>();

                        object? page;

                        switch (request.Path)
                        {
                            case "/admin/vacancies":
                            {
                                page = salesRepository
                                    .Vacancies
                                    .Page(request.Path, request.Page, request.PageSize)
                                    .IncludeGeoIndex(PrecisionMaxKm, geo);

                                break;
                            }

                            case "/admin/bookings":
                            {
                                page = salesRepository
                                    .Bookings
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