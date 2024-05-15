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

namespace BookingKata.ThirdParty;

public class PaymentCommandService
(
    ITimeService DateTime
) 
    : IPaymentCommandService
{
    public PaymentResponse RequestPayment
    (
        int referenceId,
        
        double amount, 
        string currency,
        
        long debitCardNumber, 
        string debitCardOwnerName, 
        int expire,
        int CCV,

        int vendorId, 
        int terminalId
    )
    {
        //TODO

        Console.WriteLine("FAKE PAYMENT!!!");

        var transactionTime = DateTime.UtcNow;

        var transactionId =
        (
            referenceId,
        
            amount,
            currency,

            vendorId,
            terminalId,

            transactionTime
        ).GetHashCode();

        return new PaymentResponse
        {
            Accepted = true,
            TransactionTimeUtc = transactionTime.SerializeUniversal(),
            TransactionId = transactionId
        };
    }
}