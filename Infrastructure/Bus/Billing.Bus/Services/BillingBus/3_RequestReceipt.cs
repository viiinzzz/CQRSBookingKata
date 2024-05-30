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

namespace Support.Infrastructure.Network;

public partial class BillingBus
{
    private void Verb_Is_RequestReceipt(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<ReceiptRequest>();

        var bookingId = request.referenceId;

        using var scope = sp.GetScope<IMoneyRepository> (out var money);

        var referenceId = new { bookingId };

        try
        {
            var receiptId = (
                from receipt in money.Receipts
                    .Include(r => r.Invoice)
                    .ThenInclude(invoice => invoice.Quotation)

                where receipt.Invoice.Quotation.ReferenceId == bookingId

                select receipt.ReceiptId
            ).FirstOrDefault();

            var id = new Id<Receipt>(receiptId);

            Notify(new ResponseNotification(notification, Omni, ReceiptFound, id));
        }
        catch (Exception e)
        {
            Notify(new ResponseNotification(notification, Omni, ReceiptNotFound, referenceId));
        }
    }
}