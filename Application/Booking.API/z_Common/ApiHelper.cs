namespace BookingKata.API.Helpers;

public static class ApiHelper
{
    private static readonly Regex SpaceRx = new(@"\s+", RegexOptions.Multiline);


    public static IResult AsResult<TEntity>(this TEntity? result) where TEntity : class
    {
        return result == default
            ? Results.NotFound()
            : Results.Ok(result);
    }


    public static async Task<IResult> WithStackTrace(this Func<Task<IResult>> fetch)
    {
        try
        {
            return await fetch();
        }
        catch (Exception ex)
        {
            return Results.Problem(new ProblemDetails
            {
                Title = ex.Message, 
                Detail = ex.StackTrace, 
                Status = 500
            });
        }
    }

}