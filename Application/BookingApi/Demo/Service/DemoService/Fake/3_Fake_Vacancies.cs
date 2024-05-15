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

namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_Vacancies()
    {
        try
        {
            var originator = GetType().FullName
                             ?? throw new ArgumentException("invalid originator");

            void createVacancies(int hotelId)
            {
                var message = "Demo: Seeding Vacancies for hotel#{0}...";

                var args = new object[] { hotelId };

                bus.Notify(new AdvertisementNotification(message, args)
                {
                    Originator = originator,
                    Immediate = true
                });


                var season = new OpenHotelSeasonRequest
                {
                    openingDateUtc = DateTime.UtcNow.SerializeUniversal(),
                    closingDateUtc = DateTime.UtcNow.AddDays(SeasonDayNumbers).SerializeUniversal(),

                    exceptRoomNumbers = default,
                    hotelId = hotelId
                };

                var opening = bus.AskResult<HotelOpening>(Recipient.Sales, Verb.Sales.RequestOpenHotelSeason,
                    season,
                    originator);

                if (opening == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(opening));
                }
            }

            demoContext.FakeHotelsIds
                .AsParallel()
                .ForAll(createVacancies);
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Vacancies)}' failed", e);
        }
    }
}