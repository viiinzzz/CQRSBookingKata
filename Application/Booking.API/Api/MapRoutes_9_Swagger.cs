namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_9_Swagger(WebApplication app)
    {
        var demo = app.MapGroup("/test");

        demo.MapGet("/",
            () => Results.Redirect("/swagger")
        ).ExcludeFromDescription();
    }
}