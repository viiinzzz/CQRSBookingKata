namespace VinZ.Common;

public record PageResult<TEntity>
(
    string type, string elementType,
    int page, int pageSize,

    bool error, string? reason,
    int pageCount, int itemCount,
    PageLinks[] links
)
    : IHaveCollection<TEntity>
{
    private TEntity[]? items { get; set; }

    public IEnumerable<TEntity> Collection
    {
        get => items == default ? Enumerable.Empty<TEntity>() : items.AsEnumerable();

        set
        {
            var previousItemsLength = items?.Length;

            items = value.ToArray();

            if (previousItemsLength.HasValue && 
                items.Length != previousItemsLength)
            {
                throw new ArgumentException("must be same Count() as previously", nameof(Collection));
            }
        }
    }
}