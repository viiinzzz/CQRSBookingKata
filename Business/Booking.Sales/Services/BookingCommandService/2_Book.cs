
using VinZ.Common;

namespace BookingKata.Sales;

public partial class BookingCommandService
{
    public int Book
    (
        string lastName,
        string firstName,

        long debitCardNumber,
        DebitCardSecrets secrets,
        VendorIdentifiers vendor,

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
            arrivalDate = stayProposition.ArrivalDateUtc.DeserializeUniversal_ThrowIfNull(nameof(stayProposition.ArrivalDateUtc)),
            departureDate = stayProposition.DepartureDateUtc.DeserializeUniversal_ThrowIfNull(nameof(stayProposition.DepartureDateUtc)),
            hotelName = roomDetails.HotelName,
            hotelRank = roomDetails.HotelRank,
            cityName = roomDetails.NearestKnownCityName,
            latitude = roomDetails.Latitude,
            longitude = roomDetails.Longitude,

            optionStart = stayProposition.OptionStartUtc.DeserializeUniversal(DateTime.UtcNow),
            optionEnd = stayProposition.OptionEndUtc.DeserializeUniversal(System.DateTime.MaxValue),
        };

        var amount = stayProposition.Price;
        var currency = stayProposition.Currency;

        var quotationId = bus.AskResult<Id>(
            originator, Common.Services.Billing.Recipient, Common.Services.Billing.Verb.RequestQuotation,
            new QuotationRequest
            {
                price = amount,
                currency = currency,

                optionStartUtc = quotationSpec.optionStart.SerializeUniversal(),
                optionEndUtc = quotationSpec.optionEnd.SerializeUniversal(),

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
                amount = amount,
                currency = currency,

                customerId = customerId,
                quotationId = quotationId.id
            });

        if (invoiceId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(quotationId));
        }


        var receiptId = bus.AskResult<Id>(
            originator, Common.Services.Billing.Recipient, Common.Services.Billing.Verb.RequestPayment,
            new PaymentRequest
            {
                referenceId = invoiceId.id,

                amount = amount,
                currency = currency,

                debitCardNumber = debitCardNumber,
                debitCardOwnerName = secrets.ownerName,
                debitCardExpire = secrets.expire,
                debitCardCCV = secrets.CCV,

                vendorId = vendor.vendorId,
                terminalId = vendor.terminalId
            });

        if (receiptId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(receiptId));
        }



        var room = new UniqueRoomId(stayProposition.Urid);

        var firstNight = OvernightStay.From(quotationSpec.arrivalDate);
        var lastNight = OvernightStay.FromCheckOutDate(quotationSpec.departureDate);

        var booking = new Shared.Booking
        {
            ArrivalDate = quotationSpec.arrivalDate,
            DepartureDate = quotationSpec.departureDate,
            ArrivalDayNum = firstNight.DayNum,
            DepartureDayNum = lastNight.DayNum + 1,
            NightsCount = quotationSpec.nightCount,

            Latitude = quotationSpec.latitude,
            Longitude = quotationSpec.longitude,

            HotelName = quotationSpec.hotelName,
            CityName = quotationSpec.cityName,

            LastName = lastName,
            FirstName = firstName,

            PersonCount = quotationSpec.personCount,

            Price = amount,
            Currency = currency,

            RoomNum = room.RoomNum,
            FloorNum = room.FloorNum,
            HotelId = room.HotelId,

            UniqueRoomId = stayProposition.Urid,
            CustomerId = customerId
        };

        //
        //
        sales.AddBooking(booking);
        //
        //


        var booked = firstNight.VacancyIdsUntil(lastNight, booking.UniqueRoomId);

        sales.RemoveVacancies(booked);

        bus.Notify(originator, new Notification(Omni, BookConfirmed)
        {
            Message = booking
        });

        return booking.BookingId;
    }
}