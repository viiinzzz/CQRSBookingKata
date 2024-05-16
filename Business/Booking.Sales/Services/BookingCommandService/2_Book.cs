/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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

        var roomDetails = bus.AskResult<RoomDetails>(Recipient.Admin, Verb.Admin.RequestSingleRoomDetails,
            new Id<RoomRef>(stayProposition.Urid), originator);

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

        var quotationId = bus.AskResult<Id<QuotationRequest>>(Support.Services.Billing.Recipient, Support.Services.Billing.Verb.RequestQuotation,
            new QuotationRequest
            {
                price = amount,
                currency = currency,

                optionStartUtc = quotationSpec.optionStart.SerializeUniversal(),
                optionEndUtc = quotationSpec.optionEnd.SerializeUniversal(),

                jsonMeta = System.Text.Json.JsonSerializer.Serialize(quotationSpec),

                referenceId = stayPropositionId
            }, originator);

        if (quotationId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(quotationId));
        }

        var invoiceId = bus.AskResult<Id<InvoiceRequest>>(Support.Services.Billing.Recipient, Support.Services.Billing.Verb.RequestInvoice,
            new InvoiceRequest
            {
                amount = amount,
                currency = currency,

                customerId = customerId,
                quotationId = quotationId.id
            }, originator);

        if (invoiceId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(quotationId));
        }


        var receiptId = bus.AskResult<Id<ReceiptRequest>>(Support.Services.Billing.Recipient, Support.Services.Billing.Verb.RequestPayment,
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
            }, originator);

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

        bus.Notify(new ResponseNotification(Omni, BookConfirmed, booking)
        {
            Originator = originator
        });

        return booking.BookingId;
    }
}