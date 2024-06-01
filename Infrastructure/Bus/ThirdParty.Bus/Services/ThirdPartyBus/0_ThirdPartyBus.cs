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

public partial class ThirdPartyBus(IScopeProvider sp) : MessageBusClientBase
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
                    case RequestPayment:
                    {
                        Verb_Is_RequestPayment(notification);
                        break;
                    }

                    case RequestPricing:
                    {
                        Verb_Is_RequestPricing(notification);
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
                Notify(notification.Response(ex));
            }
        };
    }
}