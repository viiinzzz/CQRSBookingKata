namespace VinZ.Common;

public class MyDbContext : DbContext
{
    public bool IsDebug { get; set; } = false;
    public LogLevel logLevel { get; set; } = LogLevel.Warning;
}