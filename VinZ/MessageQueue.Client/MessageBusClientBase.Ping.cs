// using System.Net.NetworkInformation;
// using LibPing.Net;

namespace VinZ.MessageQueue;

public partial class MessageBusClientBase
{
    private async Task ping()
    {
        ;//disabled ping
        // await ping3();
    }

    /*
    private async Task ping1()
    {
        var ping = new Ping(); //System.Net.NetworkInformation.Ping works only on Windows. Not crossplatform!!!

        var remoteIp = Dns.GetHostAddresses(_remoteHost)
            .MinBy(a => a.AddressFamily == AddressFamily.InterNetwork ? 4 :
                a.AddressFamily == AddressFamily.InterNetworkV6 ? 6 : int.MaxValue);

        if (remoteIp == null)
        {
            throw new NullReferenceException(nameof(remoteIp));
        }

        var reply = await ping.SendPingAsync(remoteIp, PingTimeoutMilliseconds);


        var fail = reply.Status != IPStatus.Success;

        if (fail)
        {
            throw new InvalidOperationException($"No answer from <<<bus:{_remoteHost}>>> ({remoteIp})");
        }
    }

    private async Task ping2()
    {
        IcmpResponse? response = null;
        var cts = new CancellationTokenSource(10000);
        try
        {
            //seem not to work
            response = await Icmp.PingAsync(_remoteHost, 128, 3000, true, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            ;
        }

        var fail = response == null;

        if (fail)
        {
            throw new InvalidOperationException($"No answer from <<<bus:{_remoteHost}>>>");
        }
    }
    */

    private async Task ping3() //opt for own ping endpoint over http then, still problem with docker 'localhost' host.docker.internal
    {
        var http = new HttpClient();
        var cts = new CancellationTokenSource(10000);

        HttpResponseMessage? response = null;

        try
        {
            var isLocalhost = string.Compare(new Uri(_ping).Host, Dns.GetHostName(), StringComparison.InvariantCultureIgnoreCase) == 0;
            Console.WriteLine($"ping... {_ping} local {Dns.GetHostName()} local={isLocalhost}");

            if (isLocalhost)
            {
                return;
            }

            response = await http.GetAsync(_ping, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            ;
        }

        var fail = response is not { StatusCode: HttpStatusCode.Accepted };

        if (fail)
        {
            throw new InvalidOperationException($"No answer from <<<bus:{_remoteHost}>>>");
        }
    }
}