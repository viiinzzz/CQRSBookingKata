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

public partial class DemoBus
(
    DemoContextService demoContextService,
    IMessageBus bus,
    ITimeService DateTime,

    //direct plug into these repo/services below is a testing purpose shortcut that shall not be used in production
    // IAdminRepository admin,
    // IMoneyRepository money,
    // ISalesRepository sales,
    // IGazetteerService geo,
    // SalesQueryService sales2,
    // BookingCommandService booking,

    IScopeProvider sp
)
{
    private const int StaffPerHotel = 1;//3;
    private const int ManagerPerHotel = 1;
    private const int HotelCount = 1;//3;
    private const int FloorPerHotel = 1;//2;
    private const int RoomPerFloor = 3;
    private const int PersonPerRoom = 2;

    private const int SeasonDayNumbers = 30;//250
    private const int CustomerCount = 10;//1000


    public async Task Execute(CancellationToken cancel)
    {
        try
        {
         

            DateTime.Freeze();

            Seed();
            demoContextService.SeedComplete = true;

            cancel.ThrowIfCancellationRequested();
        }
        catch (Exception ex)
        {
            var childNotification = new RequestNotification(nameof(Demo), nameof(Seed));

            bus.Notify(new NegativeResponseNotification(childNotification, ex, "aborted!")
            {
                Originator = originator,
                Immediate = true
            });
        }
    }

}