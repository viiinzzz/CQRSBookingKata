namespace VinZ.Common;

public class MyDbContext : DbContext
{
    public bool IsDebug { get; set; } = false;
    public bool IsTrace { get; set; } = false;
}