using BookingKata.Services;
using BookingKata.Shared;
using Common.Infrastructure.Network;
using Newtonsoft.Json;

namespace BookingKata.Sales;

public class BookingCommandService
(
    ISalesRepository sales,

    // AdminQueryService admin,//should not use that way for microservicing
    IMessageBus bus,//wanna collaborate with other microservices
    // IMoneyRepository money,
    // IPlanningRepository planning,
    // IPaymentCommandService payment,

    IGazetteerService geo,
    ITimeService DateTime
)
{
    public void OpenHotelSeason(int hotelId, int[]? exceptRoomNumbers, DateTime openingDate, DateTime closingDate)
    {
        var originator = this.GetType().FullName;


        var hotelGeoProxy = bus.AskResult<GeoProxy>(
            originator, Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id(hotelId));

        if (hotelGeoProxy == null)
        {
            throw new HotelNotFoundException();
        }


        var roomDetails = bus.AskResult<RoomDetails[]>(
            originator, Recipient.Admin, Verb.Admin.RequestRoomDetails, 
            new RoomDetailsRequest(hotelId, exceptRoomNumbers));

        if (roomDetails is null or { Length: 0 })
        {
            throw new RoomNotFoundException();
        }
        

        var firstNight = OvernightStay.From(openingDate);
        var lastNight = OvernightStay.FromCheckOutDate(closingDate);

        var dayNumbers = firstNight.DayNum.RangeTo(lastNight.DayNum);


        var vacancies = roomDetails

            .SelectMany(roomDetail => dayNumbers

                .Select(dayNum => new Vacancy(dayNum,

                    roomDetail.PersonMaxCount, roomDetail.Latitude, roomDetail.Longitude,

                    roomDetail.HotelName, roomDetail.NearestKnownCityName, false, roomDetail.Urid)
                )
            ).ToList();


        geo.CopyToReferers(hotelGeoProxy, vacancies);

        sales.AddVacancies(vacancies);
    }

    public int Book
    (
        string lastName, string firstName, long debitCardNumber, DebitCardSecrets secrets,

        int customerId,
        int stayPropositionId,
        long correlationId1,
        long correlationId2
    )
    {
        var customer = sales.GetCustomer(customerId);

        if (customer == default)
        {
            throw new CustomerNotFoundException();
        }

        if (customer.Disabled)
        {
            throw new AccountLockedException();
        }


        var originator = GetType().FullName 
                         ?? throw new Exception("invalid originator");

        var stayProposition = sales.Propositions
            .FirstOrDefault(proposition => proposition.StayPropositionId == stayPropositionId);

        if (stayProposition == null)
        {
            throw new PropositionNotFoundException();
        }

        var roomDetails = bus.AskResult<RoomDetails>(
            originator, Recipient.Admin, Verb.Admin.RequestSingleRoomDetails,
            new Id(stayProposition.Urid));

        if (roomDetails == null)
        {
            throw new RoomNotFoundException();
        }

        var quotationSpec = new
        {
            personCount = stayProposition.PersonCount,
            nightCount = stayProposition.NightsCount,
            arrivalDate = stayProposition.ArrivalDate,
            departureDate = stayProposition.DepartureDate,
            hotelName = roomDetails.HotelName,
            hotelRank = roomDetails.HotelRank,
            cityName = roomDetails.NearestKnownCityName,
            latitude = roomDetails.Latitude,
            longitude = roomDetails.Longitude
        };

        var quotationId = bus.AskResult<Id>(
            originator, Common.Services.Billing.Recipient, Common.Services.Billing.Verb.RequestQuotation,
            new QuotationRequest
            {
                price = stayProposition.Price,
                currency = stayProposition.Currency,

                optionStartUtc = (stayProposition.OptionStartsUtc ?? DateTime.UtcNow).ToString("u"),
                optionEndUtc = (stayProposition.OptionEndsUtc ?? System.DateTime.MaxValue).ToString("u"),

                jsonMeta = System.Text.Json.JsonSerializer.Serialize(quotationSpec),

                referenceId = stayPropositionId
            });

        if (quotationId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(stayPropositionId));
        }

        var invoiceId = bus.AskResult<Id>(
            originator, Common.Services.Billing.Recipient, Common.Services.Billing.Verb.RequestInvoice,
            new InvoiceRequest
            {
                amount = stayProposition.Price,
                currency = stayProposition.Currency,

                customerId = customerId,
                quotationId = quotationId.id
            });

        if (invoiceId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(quotationId));
        }



        //
        //
        var paid = bus.AskResult<PaymentRequestResponse>(
            originator, Common.Services.ThirdParty.Recipient, Common.Services.ThirdParty.Verb.RequestPayment,
            new PaymentRequest(debitCardNumber, secrets.ownerName, secrets.expire, secrets.CCV, invoiceId.id));
        //
        //

        if (paid is not { Accepted: true})
        {
            throw new PaymentFailureException();
        }





        var receiptId = bus.AskResult<Id>(
            originator, Common.Services.Billing.Recipient, Common.Services.Billing.Verb.RequestPayment,
            new InvoiceRequest
            {
                amount = stayProposition.Price,
                currency = stayProposition.Currency,

                customerId = customerId,
                quotationId = quotationId.id
            });

        if (invoiceId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(quotationId));
        }


        var receipt = new Receipt(debitCardNumber, prop.Price, prop.Currency, DateTime.UtcNow, customerId, invoice.InvoiceId);

        //
        //
        bus.Notify(new Notification(Recipient.Billing, Billing.QuotationEmitError)
        {
            CorrelationGuid = correlationId.Guid,
            Message = new { id = quotationId }
        });
        money.AddReceipt(receipt, scoped: false);
        //
        //

        var booking = new Booking(prop.ArrivalDate, prop.DepartureDate, lastName, firstName, prop.PersonCount, prop.Urid, customerId);

        
        //
        //
        sales.AddBooking(booking);
        //
        //

        var beginDay = OvernightStay.From(booking.ArrivalDate);
        var endDay = OvernightStay.FromCheckOutDate(booking.DepartureDate);

        var booked = beginDay.StayUntil(endDay, booking.UniqueRoomId);

        sales.RemoveVacancies(booked);


        bus.Notify(originator, new Notification(Omni, BookConfirmed)
        {
            Message = new NewBooking
            (
                booking.BookingId,
                booking.ArrivalDate,
                booking.DepartureDate,
                booking.LastName,
                booking.FirstName,
                booking.UniqueRoomId
            )
        });

        var room = new UniqueRoomId(booking.UniqueRoomId);

        planning.Add(new ReceptionCheck(beginDay.DayNum,
            booking.ArrivalDate, ReceptionEventType.CheckIn, 
            booking.LastName, booking.FirstName,
            room.RoomNum, false, room.HotelId,
            booking.BookingId, default, false, default, 0));

        planning.Add(new ReceptionCheck(endDay.DayNum + 1,
            booking.DepartureDate, ReceptionEventType.CheckOut, 
            booking.LastName, booking.FirstName,
            room.RoomNum, false, room.HotelId,
            booking.BookingId, default, false, default, 0));


        var departureFracDayNum = OvernightStay.From(booking.DepartureDate).DayNum;

        planning.Add(new RoomServiceDuty(
            booking.DepartureDate, System.DateTime.MaxValue,
            booking.DepartureDate.FractionalDayNum(), System.DateTime.MaxValue.FractionalDayNum(),
            room.RoomNum, room.FloorNum, false, room.HotelId, 
            booking.BookingId, default, 0));

        scope?.Complete();

        return booking.BookingId;
       
    }

  
}