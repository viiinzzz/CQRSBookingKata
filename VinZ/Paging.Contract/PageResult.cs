using System.Runtime.CompilerServices;

namespace VinZ.Common;

public record PageResult<TEntity>
(
    int page,
    int pageSize,

    bool error,
    string? reason,

    int pageCount,
    int itemCount,

    PageLinks[] links
)
    : IHaveCollection<TEntity>
{
    public string? _type { get; } = typeof(PageResult<TEntity>).FullName;
    public string? _itemType { get; } = typeof(TEntity).Name;

    public TEntity[]? items { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public IEnumerable<TEntity> Collection
    {
        get => this.GetCollection();
        set => this.SetCollection(value);
    }
}