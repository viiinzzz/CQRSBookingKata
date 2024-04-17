namespace BookingKata.Services;

public static class Recipient
{
    public const string Audit = nameof(Audit);
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
        private const string Hotel = nameof(Hotel);
        private const string HotelGeoProxy = $"{nameof(Hotel)}{nameof(GeoProxy)}";
        private const string RoomDetails = nameof(RoomDetails);
        private const string SingleRoomDetails = nameof(SingleRoomDetails);


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
        
        public const string RequestFetchHotel = $"{RequestFetch}{Hotel}";
        public const string RequestFetchHotelGeoProxy = $"{RequestFetch}{HotelGeoProxy}";
        public const string HotelFetched = $"{Hotel}{Fetched}";
        
        public const string RequestModifyHotel = $"{RequestModify}{Hotel}";
        public const string HotelModified = $"{Hotel}{Modified}";
        
        public const string RequestDisableHotel = $"{RequestDelete}{Hotel}";
        public const string HotelDisabled = $"{Hotel}{Deleted}";
        

        public const string RequestRoomDetails = $"{Request}{RoomDetails}";
        public const string RequestSingleRoomDetails = $"{Request}{SingleRoomDetails}";
        public const string RespondRoomDetails = $"{Request}{RoomDetails}";
        public const string RespondSingleRoomDetails = $"{Request}{SingleRoomDetails}";
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

        public const string RequestBook = $"{nameof(RequestBook)}";
        public const string BookConfirmed = $"{nameof(BookConfirmed)}";
        public const string BookCancelled = $"{nameof(BookCancelled)}";

        public const string RequestStay = $"{nameof(RequestStay)}";
        public const string StayFound = $"{nameof(StayFound)}";

        public const string Kpi = $"{nameof(Kpi)}";
        public const string RequestKpi = $"{Request}{Kpi}";
        public const string RespondKpi = $"{Respond}{Kpi}";


    }
}