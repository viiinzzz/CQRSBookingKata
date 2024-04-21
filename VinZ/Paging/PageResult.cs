namespace VinZ.Common;

public record PageResult<TEntity>
(
    string type, string elementType,
    int page, int pageSize,

    bool error, string? reason,
    int pageCount, int itemCount,
    IPageLinks[] links
)
    : IPageResult<TEntity>
{
    private TEntity[]? items { get; set; }

    public IEnumerable<TEntity> Collection
    {
        get => items?.AsEnumerable()
               ?? Enumerable.Empty<TEntity>();

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