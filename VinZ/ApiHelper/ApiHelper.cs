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

public static class ApiHelper
{
    public static Type[] GetConfigurationTypes(this WebApplicationBuilder builder, string? key)
    {
        return builder.Configuration.GetConfigurationTypes(key);
    }

    public static Type[] GetConfigurationTypes(this IConfiguration config, string? key)
    {
        if (key == null)
        {
            return [];
        }

        var typesStr = config.GetValues(key);

        return typesStr
            .Select(typeStr => typeStr.GetTypeFromFullNameWithLoading()
                               ?? throw new ApplicationException($"type not found '{typeStr}'"))
            .ToArray();
    }
    public static string[] GetConfigurationValues(this WebApplicationBuilder builder, string? key)
    {
        return builder.Configuration.GetConfigurationValues(key);
    }

    public static string[] GetConfigurationValues(this IConfiguration config, string? key)
    {
        return config.GetValues(key);
    }

    public static string[] GetValues(this IConfiguration config, string? key)
    {
        if (key == null)
        {
            return [];
        }

        var values = new List<string>();
        var i = 0;

        while (true)
        {
            var value = config[$"{key}:{i++}"];

            if (value == null)
            {
                break;
            }

            values.Add(value);
        }

        return [..values];
    }

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

        return addressList
            .Select(a => a.MapToIPv4())
            .Distinct()
            .OrderBy(a => BitConverter.ToString(a.GetAddressBytes()))
            .AsParallel()
            .Where(a =>
            {
                try
                {
                    return new Ping().Send(a).Status == IPStatus.Success;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }).ToArray();
    }


    public static Uri GetAppUrlPrefix(string prefix)
    {
        var urls = (Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?? string.Empty)
            .Split(";")
            .Select(url => url.Trim())
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
            prefix = prefix[..^2];
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


    private static readonly Regex SpaceRx = new(@"\s+", RegexOptions.Multiline);


    public static IResult AsResult<TEntity>(this TEntity? result) where TEntity : class
    {
        return result == default
            ? Results.NotFound()
            : Results.Ok(result);
    }

    public static IResult AsAccepted<TEntity>(this TEntity? result) where TEntity : class
    {
      return result == default
          ? Results.NoContent()
          : Results.Accepted(null, result);
    }


    public static async Task<IResult> WithStackTrace(this Func<Task<IResult>> fetch)
    {
        try
        {
            return await fetch();
        }
        catch (Exception ex)
        {
            return Results.Problem(new ProblemDetails
            {
                Title = ex.Message, 
                Detail = ex.StackTrace, 
                Status = 500
            });
        }
    }
}