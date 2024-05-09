namespace VinZ.MessageQueue;

public partial class MqServer
{
    private NotifyAck HttpSend(string clientUrl, ClientNotification notification)
    {
        var baseAddress = new Uri(clientUrl);
        var uri = nameof(Notify).ToLower();
        var url = baseAddress + uri;

        try
        {
            var cancel = new CancellationTokenSource();

            var json = notification.ToJson();

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var http = new HttpClient
            {
                BaseAddress = baseAddress
            };


            if (_isTrace)
                log.LogInformation(@$"
<<............................................................
| HTTP POST {url} (0)
+.............................................................
| {notification.ToJson(true)}
");

            var post = http.PostAsync(uri, content, cancel.Token);

            post.Wait(cancel.Token);

            if (_isTrace)
                log.LogInformation(@$"
                        +.........................................................
                        | HTTP POST {url}
                        +............................................( {(int)post.Result.StatusCode:000} )...>>>
");

            var res = post.Result;

            var statusCode = (int)res.StatusCode;

            if (statusCode is < 200 or > 299)
            {
                return new NotifyAck
                {
                    Valid = false,
                    Status = res.StatusCode,
                    data = @$"failed to reach {url}
{res.ReasonPhrase}"
                };
            }

            var read = res.Content.ReadAsStringAsync(cancel.Token);

            read.Wait(cancel.Token);

            var ack = JsonConvert.DeserializeObject<NotifyAck>(read.Result);

            if (ack == null)
            {
                throw new NullReferenceException(nameof(ack));
            }

            return ack;
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            if (ex is TaskCanceledException cancelEx)
            {
                if (_isTrace)
                    log.LogInformation(@$"
                        +.........................................................
                        | HTTP POST {url} ({(int)HttpStatusCode.RequestTimeout})
                        +......................................................>>>
");

                return new NotifyAck
                {
                    Valid = false,
                    Status = HttpStatusCode.RequestTimeout,
                    data = @$"failed to reach {url}"
                };
            }

            if (_isTrace)
                log.LogInformation(@$"
                        +.........................................................
                        | HTTP POST {url} ({(int)HttpStatusCode.InternalServerError})
                        +......................................................>>>
");

            return new NotifyAck
            {
                Valid = false,
                Status = HttpStatusCode.InternalServerError,
                data = @$"failed to reach {url}

---sensitive
{ex.Message}
{ex.StackTrace}"
            };
        }
    }
}