namespace BookingKata.Sales;

public partial class BookingCommandService
{
    public int Book
    (
        string lastName, string firstName,
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


        var receiptId = bus.AskResult<Id>(
            originator, Common.Services.Billing.Recipient, Common.Services.Billing.Verb.RequestPayment,
            new PaymentRequest
            {
                debitCardNumber = debitCardNumber,
                debitCardOwnerName = secrets.ownerName,
                debitCardExpire = secrets.expire,
                debitCardCCV = secrets.CCV,

                vendorId = vendor.vendorId,
                terminalId = vendor.terminalId,
                invoiceId = invoiceId.id
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

            Price = stayProposition.Price,
            Currency = stayProposition.Currency,

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


        var booked = firstNight.StayUntil(lastNight, booking.UniqueRoomId);

        sales.RemoveVacancies(booked);

        bus.Notify(originator, new Notification(Omni, BookConfirmed)
        {
            Message = booking
        });

        return booking.BookingId;
    }
}