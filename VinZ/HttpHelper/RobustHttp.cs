/*
 * HttpHelper
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

using System.Net;
using System.Net.Http.Headers;
using VinZ.Common.KVStore;
using VinZ.Common.KVStore.Sqlite;
using VinZ.Common.Logging;
using VinZ.Common.Retry;
using VinZ.Common;

namespace VinZ.Common.Http;



public class RobustHttp
{
    private static RobustHttp? _instance;

    public static RobustHttp GetGlobalInstance()
        => _instance ??= new RobustHttp(ILogs.Level.Info, "global", "c:/temp/http");

    public static RobustHttp GetInstance(ILogs.Level logLevel, string cacheKey, string baseDir)
        => new(logLevel, cacheKey ?? throw new ArgumentNullException(nameof(cacheKey)), baseDir);

    public const int
        defaultTimeoutSeconds = 120,
        ResponseDebugMaxLength = 10000,
        ResponseDebugMaxLengthTruncate = 5000,
        FileSizeMax = 500_000_000,
        MemorySizeMax = 0;

    private const string
        GetSubdir = "Get";


    private readonly string GetCacheDir;
    private readonly StringCache GetCache;
    private readonly string CacheKey;

    public int GetCallsCount { get; private set; } = 0;
    public long GetCallsChars { get; private set; } = 0;


    private RobustHttp(ILogs.Level logLevel, string cacheKey, string baseDir)
    {
        CacheKey = cacheKey;
        GetCacheDir = Path.GetFullPath(Path.Combine(baseDir, $"{GetSubdir}-{cacheKey}"));

        var cacheConfig = new KVStoreConfig(GetCacheDir, FileSizeMax, MemorySizeMax, logLevel, DebugStore);
        GetCache = new StringCacheOnSqlite(cacheConfig);
    }



    public Logs GetLogs => GetCache.Logs;


    public (string response, string? cacheFile) GetReturn((string? response, string? cacheFile) ret)
    {
        GetCallsCount++;
        GetCallsChars += ret.response?.Length ?? 0;

        return (ret.response ?? string.Empty, ret.cacheFile);
    }





    public async Task<string> Get(string url, AuthenticationHeaderValue? authorization, int? lifetimeHours, ILogs logs, IHttpExceptionHandler exceptionHandler, CancellationToken? cancellationToken)
    {
        var (response, _) = await GetWithCache(url, authorization, lifetimeHours, logs, exceptionHandler, cancellationToken);
        
        return response;
    }

    //TODO somehow index cache with authorization like url = authscheme value@url
    public async Task<(string response, string? cacheFile)> GetWithCache(
        string url,
        AuthenticationHeaderValue? authorization,
        int? lifetimeHours,
        ILogs logs,
        IHttpExceptionHandler exceptionHandler,
        CancellationToken? cancellationToken,
        bool cacheEnabled = true,
        bool retryEnabled = true,
        int timeoutSeconds = defaultTimeoutSeconds
    )
    {
        var authorizationHash = authorization == default ? default :  $"{$"{authorization.Scheme} {authorization.Parameter}".GetHashCode64():x16}";
        var url2 = authorizationHash == default ? url : $"{authorizationHash}@{url}";
            
        var retryer = new Retryer(logs, Retryer.Arguments.Default with
        {
            debug = $"GET {url}",
            MaxWaitMilliseconds = (timeoutSeconds + 5) * 1000,
            RetryMilliseconds = (int)Math.Ceiling(timeoutSeconds * 1000 * 0.2),
            RetryCount = 3,
            RetryDelayFactor = 1.33,
            StopExceptions = new [] { typeof(HttpAbortException) }
        });

        var getWithoutCache = async (CancellationToken? cancel) =>
        {
            var options = new GetBaseOptions(logs, exceptionHandler, timeoutSeconds);

            var response = await GetBase(url, authorization, options, cancel ?? CancellationToken.None);

            if (DebugHttp) logs.Always($"> GET {url}");

            return response;
        };

        var getWithoutCacheWithRetry = async (CancellationToken? cancel) =>
        {
            return await retryer.Run<string>(
                async (retryCancellation) => await getWithoutCache(retryCancellation),
                cancel);
        };


        if (!cacheEnabled)
        {
            if (!retryEnabled)
            {
                return (await getWithoutCache(cancellationToken), null);
            }

            return (await getWithoutCacheWithRetry(cancellationToken), null);
        }


        //
        //
        (string? value, string filePath) cache = default;
        try
        {
            cache = await GetCache.Read(url2, cancellationToken);
        }
        catch (Exception ex)
        {
            //TODO
            //yes shame there is
            //the cache is fallible, hence if it did, ignore
            
            //have switch from file based to sqlite
            // not anymore such conflicts :-)

            logs?.Warn($"Cache is currently not available: {ex.Message}");
        }

        //
        //
        cancellationToken?.ThrowIfCancellationRequested();


        if (cache.value != null)
        {
            if (DebugHttp) logs.Always($"> CACHE {url}");

            return GetReturn(cache);
        }

        string response;

        if (!retryEnabled)
        {
            response = await getWithoutCache(cancellationToken);
        }
        else
        {
            response = await retryer.Run<string>(
                async (retryCancellation) => await getWithoutCache(retryCancellation),
                cancellationToken);
        }

        await GetCache.Write(url2, response, lifetimeHours, cancellationToken);

        return GetReturn((response, cache.filePath));
    }


    public record GetBaseOptions(ILogs Logs, IHttpExceptionHandler ExceptionHandler, int TimeoutSeconds = defaultTimeoutSeconds);

    private async Task<string> GetBase(
        string url,
        AuthenticationHeaderValue? authorization,
        GetBaseOptions options,
        CancellationToken? cancellationToken = default
        )
    {
        if (cancellationToken.HasValue)
        {
            cancellationToken.Value.ThrowIfCancellationRequested();
        }

        var logs = options.Logs;
        var exceptionHandler = options.ExceptionHandler;

        var responseText = string.Empty;
        HttpClient? http = default;
        try
        {
            http = new HttpClient();

            http.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/116.0"
            );

            http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            if (authorization != default)
            {
                http.DefaultRequestHeaders.Authorization = authorization;
            }

            //
            //
            var response = await http.GetAsync(url, cancellationToken ?? CancellationToken.None);
            //
            //

            var responseTextTask = response?.Content?.ReadAsStringAsync();
            responseText = (responseTextTask == null ? null : await responseTextTask) ?? string.Empty;

            responseText.ReplaceLineEndings(Environment.NewLine);

            var exceptionText = exceptionHandler.GetExceptionText(responseText);

            if (response?.StatusCode != HttpStatusCode.OK)
            {
                throw new ResponseCodeNotOKException(
                    response?.StatusCode,
                    exceptionText ?? $"{Environment.NewLine}{Environment.NewLine}{responseText}"
                    );
            }

            logs.Debug(@$"
Http.Get response headers:

{response.Headers}");

            var responseTextTruncate = responseText.Length > ResponseDebugMaxLength
                ? responseText.Substring(0, ResponseDebugMaxLengthTruncate)
                  + @$"{Environment.NewLine}... ({responseText.Length - ResponseDebugMaxLengthTruncate} more chars)"
                : responseText;

            logs.Debug(@$"
Http.Get response:

{responseTextTruncate}");

            return responseText;
        }
        catch (Exception e)
        {
            var errorMessage = e.Message;

            var httpClientTimeout = e is TaskCanceledException && errorMessage.Contains("HttpClient.Timeout");

            if (httpClientTimeout)
            {
                errorMessage = $"timeout {http?.Timeout.TotalSeconds ?? 0:0.0}s";
            }

            var responseTextTruncate = responseText.Length > ResponseDebugMaxLength
                ? responseText.Substring(0, ResponseDebugMaxLengthTruncate)
                  + @$"{Environment.NewLine}... ({responseText.Length - ResponseDebugMaxLengthTruncate} more chars)"
                : responseText;

            logs.Debug(@$"
Http.Get response:

{responseTextTruncate}");

            var header = $"GET {url}";

            if (httpClientTimeout)
            {
                throw new Exception($"{header}{Environment.NewLine}'{errorMessage}'");
            }

            exceptionHandler.ThrowAbort(header, e);

            throw new HttpAbortException(header, e);
        }
    }

}