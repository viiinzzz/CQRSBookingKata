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
        SessionId = RandomHelper.Long();

        logger.Log(LogLevel.Debug, 
            $"<<<Server:{Id.xby4()}>>> SessiondId {SessionId}");
    }

    public long Id { get; }
    public long SessionId { get; }
}

