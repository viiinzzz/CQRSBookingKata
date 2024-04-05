namespace VinZ.Common;

public class PageLinks
{
    public PageLinks(string baseUrl, int page, int pageSize, int pageCount)
    {
        url = $"{baseUrl}?page={page}&pageSize={pageSize}";
        next = page >= pageCount ? null : $"{baseUrl}?page={page + 1}&pageSize={pageSize}";
        prev = page <= 1 ? null : $"{baseUrl}?page={page - 1}&pageSize={pageSize}";
    }

    public string url { get; }
    public string? next { get; }
    public string? prev { get; }
}