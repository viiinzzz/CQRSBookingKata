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
    const int responseTimeoutSeconds = 120;

    public static void MapRoutes(WebApplication app, bool isMainContainer)
    {
        MapRoutes_0_Bus(app);

        if (!isMainContainer)
        {
            return;
        }

        MapRoutes_1_Admin(app, out var admin);
        {
            MapRoutes_11_Employees(admin);
            MapRoutes_12_Hotels(admin);
        }

        MapRoutes_2_Money(app);

        MapRoutes_3_Sales(app);

        MapRoutes_4_Reception(app);

        MapRoutes_5_Service(app);

        MapRoutes_6_Booking(app);

        MapRoutes_8_Demo(app);

        MapRoutes_9_Swagger(app);
    }
}