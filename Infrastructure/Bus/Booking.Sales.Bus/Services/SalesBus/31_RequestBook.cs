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

namespace BookingKata.Infrastructure.Network;

public partial class SalesBus
{
    private void Verb_Is_RequestBook(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<BookRequest>();

        var debitCardSecrets = new DebitCardSecrets(request.debitCardOwner, request.debitCardExpire, request.debitCardCCV);

        var vendor = new VendorIdentifiers(request.vendorId, request.terminalId);

        using var scope = sp.GetScope<BookingCommandService>(out var booking);

        //
        //
        var bookingId = booking.Book
        (
            request.lastName,
            request.firstName,

            request.debitCardNumber,
            debitCardSecrets,
            vendor,

            request.customerId,
            request.stayPropositionId,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        var id = new Id<Shared.Booking>(bookingId);

        Notify(new ResponseNotification(Omni, Verb.Sales.BookConfirmed, id)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}