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

namespace Support.Infrastructure.Storage;

public partial class MoneyRepository
{
    public IQueryable<Quotation> Quotations

        => _money.Quotations
            .AsNoTracking();

    public int AddQuotation(Quotation quotation)
    {
        _money.Quotations.Add(quotation);
        _money.SaveChanges();
        _money.Entry(quotation).State = EntityState.Detached;

        return quotation.QuotationId;
    }

    public void UpdateQuotation(int quotationId, Quotation quotationUpdate)
    {
        if (quotationId != quotationUpdate.QuotationId)
        {
            throw new ArgumentException(ReferenceMismatch, nameof(quotationId));
        }

        var quotation = _money.Quotations
            .Find(quotationId);

        if (quotation == default)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(quotationId));
        }

        if (quotation.ReferenceId != quotationUpdate.ReferenceId)
        {
            throw new ArgumentException(ReferenceMismatch, nameof(quotationUpdate.ReferenceId));
        }


        _money.Entry(quotation).State = EntityState.Detached;


        var entity = _money.Quotations.Update(quotationUpdate);
        _money.SaveChanges();
        entity.State = EntityState.Detached;
    }
}