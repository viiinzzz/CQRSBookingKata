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

public partial class BillingBus(IScopeProvider sp) : MessageBusClientBase
{
    public override async Task Configure()
    {
        // await
        Subscribe(Recipient);

        Notified += (sender, notification) =>
        {
            try
            {
                switch (notification.Verb)
                {
                    case RequestQuotation:
                    {
                        Verb_Is_RequestQuotation(notification);
                        break;
                    }
                    case RequestInvoice:
                    {
                        Verb_Is_RequestInvoice(notification);
                        break;
                    }
                    case RequestPayment:
                    {
                        Verb_Is_RequestPayment(notification);
                        break;
                    }
                    case RequestReceipt:
                    {
                        Verb_Is_RequestReceipt(notification);
                        break;
                    }
                    case RequestRefund:
                    {
                        Verb_Is_RequestRefund(notification);
                        break;
                    }
                    case RequestPayroll:
                    {
                        Verb_Is_RequestPayroll(notification);
                        break;
                    }
                    case RequestPage:
                    {
                        Verb_Is_RequestPage(notification);
                        break;
                    }

                    default:
                    {
                        throw new VerbInvalidException(notification.Verb);
                    }
                }
            }
            catch (Exception ex)
            {
                Notify(new NegativeResponseNotification(notification, ex));
            }
        };
    }
}