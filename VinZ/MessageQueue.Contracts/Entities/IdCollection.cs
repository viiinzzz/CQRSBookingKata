namespace VinZ.Common;

public record IdCollection<TEntity>(int[] ids)
{
    public string _type { get; } = typeof(TEntity).Name;
}