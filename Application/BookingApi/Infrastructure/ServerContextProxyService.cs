namespace BookingKata.API.Infrastructure;

public class ServerContextProxyService(IMessageBus mq)
    : IServerContextService
{
    private ServerContext GetContext()
    {
            var serverContextTask = mq.Ask<ServerContext>(
                                        nameof(ServerContextProxyService), Recipient.Admin, Verb.Admin.RequestServerContext, null,
                                        CancellationToken.None, 30)
                              ?? throw new NullReferenceException();

            serverContextTask.Wait();

            return serverContextTask.Result
                   ?? throw new NullReferenceException();
    } 
    
    public long Id => GetContext()?.ServerContextId
                      ?? throw new NullReferenceException();
    
    public long SessionId => GetContext()?.SessionId
                             ?? throw new NullReferenceException();
}