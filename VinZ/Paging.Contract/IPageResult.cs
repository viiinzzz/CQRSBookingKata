// ReSharper disable InconsistentNaming
namespace VinZ.Common;

public interface IPageResult<TEntity> : IHaveCollection<TEntity>
{
    string type { get; }
    string elementType { get; }
    int page { get; }
    int pageSize { get; }

    bool error { get; }
    string? reason { get; }
    int pageCount { get; }
    int itemCount { get; }
    IPageLinks[] links { get; }

    IEnumerable<TEntity> Collection { get; set; }
}