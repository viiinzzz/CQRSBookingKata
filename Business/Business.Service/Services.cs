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

namespace BookingKata.Services;

public static class Recipient
{
    public const string Time = nameof(Time);

    public const string Admin = nameof(Admin);
    public const string Planning = nameof(Planning);
    public const string Sales = nameof(Sales);
}

public static class Verb
{


    public static class Admin
    {
        private const string Employee = nameof(Employee);
        public const string Hotel = nameof(Hotel);
        private const string Floor = nameof(Floor);
        private const string Rooms = nameof(Rooms);
        private const string FloorRooms = $"{nameof(Floor)}{nameof(Rooms)}";
        private const string HotelGeoProxy = $"{nameof(Hotel)}{nameof(GeoProxy)}";
        private const string HotelRoomDetails = nameof(HotelRoomDetails);
        private const string SingleRoomDetails = nameof(SingleRoomDetails);
        private const string ManyRoomDetails = nameof(ManyRoomDetails);

        public const string RequestTimeForward = nameof(RequestTimeForward);

        public const string RequestCreateEmployee = $"{RequestCreate}{Employee}";
        public const string EmployeeCreated = $"{Employee}{Created}";
        
        public const string RequestFetchEmployee = $"{RequestFetch}{Employee}";
        public const string EmployeeFetched = $"{Employee}{Fetched}";
        
        public const string RequestModifyEmployee = $"{RequestModify}{Employee}";
        public const string EmployeeModified = $"{Employee}{Modified}";
        
        public const string RequestDisableEmployee = $"{RequestDelete}{Employee}";
        public const string EmployeeDisabled = $"{Employee}{Deleted}";
        

        public const string RequestCreateHotel = $"{RequestCreate}{Hotel}";
        public const string HotelCreated = $"{Hotel}{Created}";

        public const string RequestCreateFloorRooms = $"{RequestCreate}{FloorRooms}";
        public const string FloorRoomsCreated = $"{FloorRooms}{Created}";
        
        public const string RequestFetchHotel = $"{RequestFetch}{Hotel}";
        public const string RequestFetchHotelGeoProxy = $"{RequestFetch}{HotelGeoProxy}";
        public const string HotelFetched = $"{Hotel}{Fetched}";
        public const string HotelGeoProxyFetched = $"{HotelGeoProxy}{Fetched}";
        
        public const string RequestModifyHotel = $"{RequestModify}{Hotel}";
        public const string HotelModified = $"{Hotel}{Modified}";
        
        public const string RequestDisableHotel = $"{RequestDelete}{Hotel}";
        public const string HotelDisabled = $"{Hotel}{Deleted}";
        

        public const string RequestHotelRoomDetails = $"{Request}{HotelRoomDetails}";
        public const string RequestSingleRoomDetails = $"{Request}{SingleRoomDetails}";
        public const string RequestManyRoomDetails = $"{Request}{ManyRoomDetails}";
        public const string RespondHotelRoomDetails = $"{Respond}{HotelRoomDetails}";
        public const string RespondSingleRoomDetails = $"{Respond}{SingleRoomDetails}";
        public const string RespondManyRoomDetails = $"{Respond}{ManyRoomDetails}";
    } 
    
    public static class Planning
    {
        public const string RequestSomething = $"{Recipient.Planning}:{nameof(RequestSomething)}";
    } 
    
    public static class Sales
    {
        public const string OpenHotelSeason = $"{nameof(OpenHotelSeason)}";

        public const string RequestOpenHotelSeason = $"{Request}{OpenHotelSeason}";
        public const string HotelSeasonOpening = $"{nameof(HotelSeasonOpening)}";

        public const string Customer = $"{nameof(Customer)}";
        public const string RequestCreateCustomer = $"{RequestCreate}{Customer}";
        public const string CustomerCreated = $"{Customer}{Created}";

        public const string RequestBooking = $"{nameof(RequestBooking)}";
        public const string BookConfirmed = $"{nameof(BookConfirmed)}";
        public const string BookCancelled = $"{nameof(BookCancelled)}";

        public const string RequestStay = $"{nameof(RequestStay)}";
        public const string StayFound = $"{nameof(StayFound)}";

        public const string Kpi = $"{nameof(Kpi)}";
        public const string HotelChain = $"{nameof(HotelChain)}";
        public const string HotelKpi = $"{Admin.Hotel}{Kpi}";
        public const string HotelChainKpi = $"{HotelChain}{Kpi}";
        public const string RequestHotelKpi = $"{Request}{HotelKpi}";
        public const string RequestHotelChainKpi = $"{Request}{HotelChainKpi}";
        public const string RespondHotelKpi = $"{Respond}{HotelKpi}";
        public const string RespondHotelChainKpi = $"{Respond}{HotelChainKpi}";


    }
}