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

namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public Id EmitQuotation
    (
        double price,
        string currency,
        DateTime optionStartUtc,
        DateTime optionEndUtc,
        string jsonMeta,

        int referenceId,
        long correlationId1,
        long correlationId2
    )
    {
        var previousQuotation = money.Quotations
            .FirstOrDefault(quotation => quotation.ReferenceId == referenceId);

        var quotation = new Quotation
        {
            Price = price,
            Currency = currency,
            OptionStartsUtc = optionStartUtc,
            OptionEndsUtc = optionEndUtc,
            jsonMeta = jsonMeta,
            ReferenceId = referenceId,
            VersionNumber = previousQuotation == null ? 1 : previousQuotation.VersionNumber + 1,
            CorrelationId1 = correlationId1,
            CorrelationId2 = correlationId2
        };

        if (previousQuotation == null)
        {
            return new Id(money.AddQuotation(quotation));
        }

        var quotationId = previousQuotation.QuotationId;

        if (previousQuotation with { VersionNumber = 0 } == quotation with { VersionNumber = 0 })
        {
            return new Id(quotationId);
        }

        money.UpdateQuotation(quotationId, quotation);

        return new Id(quotationId);
    }
}