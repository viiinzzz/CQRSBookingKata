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


    public static TEntity Patch<TEntity, TPartEntity>(this TEntity current, TPartEntity patch)
        where TEntity : class
        where TPartEntity : class
    {
        if (current == null)
        {
            throw new ArgumentNullException(nameof(current));
        }

        var retObj = JObject.FromObject(current);
        var patchObj = JObject.FromObject(patch);

        retObj?.Merge(patchObj, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });

        return retObj?.ToObject<TEntity>()

               ?? throw new ArgumentNullException(nameof(retObj));
    }

 


}