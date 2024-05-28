/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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
                return notification.Ack() with
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

                return notification.Ack() with
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

            return notification.Ack() with
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