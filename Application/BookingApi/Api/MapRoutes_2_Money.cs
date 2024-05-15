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

namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string MoneyTag = "Money";

    private static void MapRoutes_2_Money(WebApplication app)
    {
        var money = app.MapGroup("/money"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

        var payrolls = money.MapGroup("/payrolls"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, MoneyTag]);

        var invoices = money.MapGroup("/invoices"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, MoneyTag]);


        payrolls.MapListMq<Payroll>("/", "/money/payrolls", filter: null,
            Support.Services.Billing.Recipient, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, MoneyTag]);

        invoices.MapListMq<Invoice>("/", "/money/invoices", filter: null,
            Support.Services.Billing.Recipient, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, MoneyTag]);

    }
}