namespace BookingKata.API;

public static partial class ApiMethods
{
    const int responseTimeoutSeconds = 120;

    public static void MapRoutes(WebApplication app)
    {
        MapRoutes_0_Demo(app);

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
    }
}