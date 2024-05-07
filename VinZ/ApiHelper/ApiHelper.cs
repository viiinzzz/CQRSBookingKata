
namespace VinZ.Common;

public static class ApiHelper
{

    public static IPAddress[] GetMyIps()
    {
        var addressList = Dns.GetHostByName(Dns.GetHostName()).AddressList;

        return addressList
            .Select(a => a.MapToIPv4())
            .Distinct()
            .OrderBy(a => BitConverter.ToString(a.GetAddressBytes()))
            .AsParallel()
            .Where(a =>
            {
                try
                {
                    return new Ping().Send(a).Status == IPStatus.Success;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }).ToArray();
    }


    public static Uri GetAppUrl()
    {
        var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS").Split(";");
        var url1 = urls.First();
        url1 = url1.Replace("//*", "//localhost");
        var url = new Uri($"{url1}/bus/");
        if (url.IsLoopback)
        {
            var host = Dns.GetHostEntry("").HostName;
            url = new Uri($"{url.Scheme}://{host}:{url.Port}{url.PathAndQuery}");
        }

        return url;
    }


    private static readonly Regex SpaceRx = new(@"\s+", RegexOptions.Multiline);


    public static IResult AsResult<TEntity>(this TEntity? result) where TEntity : class
    {
        return result == default
            ? Results.NotFound()
            : Results.Ok(result);
    }

    public static IResult AsAccepted<TEntity>(this TEntity? result) where TEntity : class
    {
      return result == default
          ? Results.NoContent()
          : Results.Accepted(null, result);
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