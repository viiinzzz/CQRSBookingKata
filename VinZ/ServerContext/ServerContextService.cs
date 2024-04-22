using Microsoft.Extensions.Logging;

namespace VinZ.Common;

public class ServerContextService
    : IServerContextService
{
    private readonly ILogger<IServerContextService> _logger;

    public ServerContextService(ILogger<IServerContextService> logger)
    {
        _logger = logger;
        Id = UuidHelper.GetUuidInt64();

        logger.LogInformation($"<<<{Id.xby4()}>>>");

    }

    public long Id { get; }
}

