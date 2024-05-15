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
    private void Verb_Is_RequestPage(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PageRequest>();

        object? page;

        using var scope2 = sp.GetScope<IMoneyRepository>(out var moneyRepository);

        switch (request.Path)
        {
            case "/money/quotations":
            {
                page = moneyRepository
                    .Quotations
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            case "/money/invoices":
            {
                page = moneyRepository
                    .Invoices
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            case "/money/receipts":
            {
                page = moneyRepository
                    .Receipts
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            case "/money/refunds":
            {
                page = moneyRepository
                    .Refunds
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            case "/money/payrolls":
            {
                page = moneyRepository
                    .Payrolls
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            default:
            {
                throw new NotImplementedException($"page request for path not supported: {request.Path}");
            }
        }

        Notify(new ResponseNotification(page)
        {
            Originator = notification.Originator,
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}