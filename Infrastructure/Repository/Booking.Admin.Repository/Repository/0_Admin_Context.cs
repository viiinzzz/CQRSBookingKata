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

using BookingKata.Shared;

namespace BookingKata.Infrastructure.Storage;

public partial class AdminRepository(
    IDbContextFactory factory,
    IGazetteerService geo,
    BookingConfiguration bconf
    // ITimeService DateTime
    ) : IAdminRepository, ITransactionable
{
    private readonly BookingAdminContext _admin = factory.CreateDbContext<BookingAdminContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _admin;
}
