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

namespace VinZ.Common;

public static partial class ApiHelper
{

    public static (bool isDevelopment, bool isStaging, bool isProduction, string? env) GetEnv(this WebApplication api)
    {
        var isDevelopment = api.Environment.IsDevelopment();
        var isStaging = api.Environment.IsStaging();
        var isProduction = api.Environment.IsProduction();

        var env = isDevelopment ? "Development" : isStaging ? "Staging" : isProduction ? "Production" : null;

        return (isDevelopment, isStaging, isProduction, env);
    }


    public static IPAddress[] GetMyIps()
    {
        var hostName = Dns.GetHostName();

        var addressList = Dns.GetHostByName(hostName).AddressList;

        IPAddress ip0;
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            ip0 = endPoint.Address;
        }
        // Console.Error.WriteLine($"localIP={ip0}");
        //
        // foreach (var a in addressList)
        // {
        //     Console.Error.WriteLine($"address={a}");
        // }

        var ip1 = addressList
            .Select(a => a.MapToIPv4())
            .Distinct()
            .OrderBy(a => BitConverter.ToString(a.GetAddressBytes()))
            // .AsParallel()
            // .Where(a =>
            // {
            //     try
            //     {//ping only works well on Windows -- don't use
            //         return new Ping().Send(a).Status == IPStatus.Success;
            //     }
            //     catch (Exception ex)
            //     {
            //         return false;
            //     }
            // })
            .ToArray();

        var ret = ip1.Append(ip0).Distinct().ToArray();

        // Console.Error.WriteLine($"ret={string.Join(", ", ret.Select(a => a.ToString()))}");

        return ret;
    }


    public static Uri GetAppUrlPrefix(string prefix)
    {
        var urls = (Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?? string.Empty)
            .Split(";")
            .Select(url => url.Trim())
            .Where(url => url.Length > 0)
            .ToArray();

        if (urls.Length == 0)
        {
            throw new ApplicationException("Missing environment variable ASPNETCORE_URLS=http://*:5291 for example");
        }

        var url1 = urls.First();
        
        url1 = url1.Replace("//*", "//localhost");

        if (prefix.StartsWith('/'))
        {
            prefix = prefix[1..];
        }

        if (prefix.EndsWith('/'))
        {
            prefix = prefix[..^1];
        }

        try
        {
            var url = new Uri($"{url1}/{prefix}/");

            if (!url.IsLoopback)
            {
                return url;
            }

            var host = Dns.GetHostEntry("").HostName;

            return new Uri($"{url.Scheme}://{host}:{url.Port}{url.PathAndQuery}");
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Invalid environment variable ASPNETCORE_URLS={string.Join(";", urls)}");
        }
    }
}