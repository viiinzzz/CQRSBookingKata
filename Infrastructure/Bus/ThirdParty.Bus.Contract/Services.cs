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

namespace Support.Services;

public static class ThirdParty
{
    public const string Recipient = nameof(ThirdParty);

    public static class Verb
    {
        public const string RequestPricing = $"{Recipient}.{nameof(RequestPricing)}";
        public const string RespondPricing = $"{Recipient}.{nameof(RespondPricing)}";

        public const string RequestPayment = $"{Recipient}.{nameof(RequestPayment)}";
        public const string PaymentAccepted= $"{Recipient}.{nameof(PaymentAccepted)}";
        public const string PaymentRefused= $"{Recipient}.{nameof(PaymentRefused)}";
    }
}