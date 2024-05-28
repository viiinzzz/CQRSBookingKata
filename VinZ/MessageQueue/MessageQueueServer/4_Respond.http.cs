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

using System.Drawing;

namespace VinZ.MessageQueue;

public partial class MqServer
{


    private static readonly string scheme = $"{Bg(Color.Gray)}{Fg(Color.Black)}HTTP{Rs}";

    private static readonly string postMethod = $"{Bg(Color.MediumSeaGreen)}{Fg(Color.Black)}POST{Rs}";




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
| {scheme}{postMethod} {Href(url)}
+...................................................( {0:000} )...
{notification.ToJson(true)}
...");

            var post = http.PostAsync(uri, content, cancel.Token);

            post.Wait(cancel.Token);

            var res = post.Result;
            var statusCode = (int)res.StatusCode;
            var statusOk = statusCode is >= 200 and < 300;

            if (_isTrace)
                log.LogInformation(@$"
                        +.........................................................
                        | {scheme}{postMethod} {Href(url)}
                        +............................................( {Fg(statusOk ? Color.Green : Color.Red)}{statusCode:000}{Rs} )...>>>
");


            if (!statusOk)
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
                        | {scheme}{postMethod} {Href(url)}
                        +............................................( {Fg(Color.Orange)}{(int)HttpStatusCode.RequestTimeout:000}{Rs} )...>>>
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
                        | {scheme}{postMethod} {Href(url)}
                        +............................................( {Fg(Color.Red)}{(int)HttpStatusCode.InternalServerError:000}{Rs} )...>>>
");

            return notification.Ack() with
            {
                Valid = false,
                Status = HttpStatusCode.InternalServerError,
                data = ex is HttpRequestException 
                    ? @$"failed to reach {url}

{ex.Message}"
                    : @$"failed to reach {url}

!!! sensitive
{ex.Message}
{ex.StackTrace}
!!!"
            };
        }
    }



    //ANSI
    //
    private const string ESC = "\u001b";
    private const string CSI = $"{ESC}[";
    private static string SGR(params byte[] codes) => $"{CSI}{string.Join(";", codes.Select(c => c.ToString()))}m";

    private static readonly string Rs = SGR(39, 49); //0
    private static readonly string Bold = SGR(1);
    private static readonly string Faint = SGR(2);
    private static readonly string Italic = SGR(3);
    private static readonly string Underlined = SGR(4);
    private static readonly string Blink = SGR(5);
    private static readonly string Inverted = SGR(7);
    private static readonly string StrikeThrough = SGR(9);
    private static readonly string Overlined = SGR(53);

    private static string Fg(Color color) => SGR(38, 2, color.R, color.G, color.B);
    private static string Bg(Color color) => SGR(48, 2, color.R, color.G, color.B);
    private static string Href(string link, string? text = null) => $"{ESC}]8;;{link}\a{text ?? link}{ESC}]8;;\a{Rs}"; //hyperlink

    private static readonly string UpAndClearStr = $"{CSI}1A{CSI}2K";
    private static readonly string BoldAndBlinkStr = $"{CSI}1{CSI}5";

    //
    //
}