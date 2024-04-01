using VinZ.ToolBox;

namespace VinZ.Paging;

public record PageResult<TEntity>(

    int page, int pageSize, int pageCount, int itemCount,

    PageLinks[] links,

    string type, string elementType

) : IHaveCollection<TEntity>
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