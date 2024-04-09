
namespace BookingKata.Services;

public static class Recipient
{
    public const string Audit = nameof(Audit);
    public const string Time = nameof(Time);

    public const string Admin = nameof(Admin);
    public const string Planning = nameof(Planning);
    public const string Sales = nameof(Sales);
    // public const string Billing = nameof(Billing);
}

public static class Verb
{


    public static class Admin
    {
        
        public const string RequestCreation = $"{Recipient.Admin}:{nameof(RequestCreation)}";
        public const string Created = $"{Recipient.Admin}:{nameof(Created)}";
        
        public const string RequestRoomDetails = $"{Recipient.Admin}:{nameof(RequestRoomDetails)}";
        public const string RespondRoomDetails = $"{Recipient.Admin}:{nameof(RespondRoomDetails)}";
    } 
    
    public static class Planning
    {
        public const string RequestSomething = $"{Recipient.Planning}:{nameof(RequestSomething)}";
    } 
    
    public static class Sales
    {
        public const string RequestOpenHotelSeason = $"{Recipient.Sales}:{nameof(RequestOpenHotelSeason)}";
        public const string HotelSeasonOpening = $"{Recipient.Sales}:{nameof(HotelSeasonOpening)}";

        public const string RequestBook = $"{Recipient.Sales}:{nameof(RequestBook)}";
        public const string BookConfirmed = $"{Recipient.Sales}:{nameof(BookConfirmed)}";

        public const string RequestKpi = $"{Recipient.Sales}:{nameof(RequestKpi)}";
        public const string RespondKpi = $"{Recipient.Sales}:{nameof(RespondKpi)}";


    }
}