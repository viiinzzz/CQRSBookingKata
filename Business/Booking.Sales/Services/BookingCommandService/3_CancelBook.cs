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
    public void CancelBook
    (
        int bookingId,
        long correlationId1,
        long correlationId2
    )
    {
        var cancelDate = DateTime.UtcNow;
        var cancelDayNum = OvernightStay.From(cancelDate).DayNum;

        var booking = (
            from b in sales.Bookings
            where b.BookingId == bookingId &&
                  !b.Cancelled &&
                  b.ArrivalDayNum >= cancelDayNum
            select b
        ).FirstOrDefault();

        if (booking == default)
        {
            throw new CancellableBookingNotFoundException();
        }


        var customer = sales.GetCustomer(booking.CustomerId);

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

        var receiptId = bus.AskResult<Id>(Support.Services.Billing.Recipient, Support.Services.Billing.Verb.RequestReceipt,
            new ReceiptRequest
            {
                referenceId = bookingId
            }, originator);

        if (receiptId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(bookingId));
        }


        var roomDetail = bus.AskResult<RoomDetails>(Recipient.Admin, Verb.Admin.RequestSingleRoomDetails,
            new Id(booking.UniqueRoomId), originator);

        if (roomDetail == null)
        {
            throw new RoomNotFoundException();
        }

        var refundId = bus.AskResult<Id>(Support.Services.Billing.Recipient, Support.Services.Billing.Verb.RequestRefund,
            new RefundRequest { receiptId = receiptId.id }, originator);

        if (refundId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(receiptId));
        }


        booking = sales.CancelBooking(bookingId);


        // var room = new UniqueRoomId(booking.UniqueRoomId);

        var firstNight = OvernightStay.From(booking.ArrivalDate);
        var lastNight = OvernightStay.FromCheckOutDate(booking.DepartureDate);


        var unbooked = 
            firstNight.DayNumssUntil(lastNight, booking.UniqueRoomId)

                .Select(dayNum => new Vacancy(dayNum,
                    roomDetail.PersonMaxCount, roomDetail.Latitude, roomDetail.Longitude,
                    roomDetail.HotelName, roomDetail.NearestKnownCityName, false, roomDetail.Urid)
                );


        sales.AddVacancies(unbooked);

        var id = new Id(bookingId);

        bus.Notify(new ResponseNotification(Omni, BookCancelled, id)
        {
            Originator = originator
        });
    }
}