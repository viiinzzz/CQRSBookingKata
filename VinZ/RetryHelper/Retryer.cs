/*
 * RetryHelper
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

using VinZ.Common.Logging;

namespace VinZ.Common.Retry;


public partial class Retryer
{


    public ILogs? Logs { get; set; }
    private readonly RetryOptions _options;

    public Retryer(ILogs? logs = default, RetryOptions? options = default)
    {
        Logs = logs;
        _options = options ?? RetryOptions.Default;
    }





    public void Run(Func<CancellationToken?, Task> func, CancellationToken? retryCancellation = default)
        => RunAsync(func, retryCancellation).Wait();


    public R? Run<R>(Func<CancellationToken?, Task<R>> func, CancellationToken? retryCancellation = default)
        => RunAsync(func, retryCancellation).Result;
    





    public async Task RunAsync(Func<CancellationToken?, Task> func, CancellationToken? retryCancellation = default)
    {
        await RunAsync(async cancel =>
        {
            await func(cancel);
            return true;
        }, retryCancellation);
    }


    public async Task<R?> RunAsync<R>(Func<CancellationToken?, Task<R>> func, CancellationToken? retryCancellation = default)
    {
        var retryMilliseconds = _options.RetryMilliseconds;
        for (int i = 0; i < _options.RetryCount; i++)
        {
            var maxWaitReached = new CancellationTokenSource(_options.MaxWaitMilliseconds);

            var cancelOrMaxWaitReached = CancellationTokenSource.CreateLinkedTokenSource(
                maxWaitReached.Token,
                retryCancellation ?? CancellationToken.None);

            try
            {
                return await func(cancelOrMaxWaitReached.Token);
            }
            catch (OperationCanceledException cancelEx)
            {
                if (cancelEx.CancellationToken != cancelOrMaxWaitReached.Token)
                {
                    if (_options.debug != default)
                    {
                        Logs?.Warn(
                            @$"{_options.debug}
retry {i + 1}/{_options.RetryCount} has been cancelled: {cancelEx.Message}
{cancelEx.StackTrace}");
                    }

                    throw cancelEx;
                }

                if (retryCancellation?.IsCancellationRequested ?? false)
                {
                    if (_options.debug != default)
                    {
                        Logs?.Warn(@$"{_options.debug}
retry {i + 1}/{_options.RetryCount} has been cancelled.");
                    }

                    return default;
                }

                if (maxWaitReached.Token.IsCancellationRequested)
                {
                    if (_options.debug != default)
                    {
                        Logs?.Warn(
                            @$"{_options.debug}
retry {i + 1}/{_options.RetryCount} has timed out after {_options.MaxWaitMilliseconds}ms");
                    }
                }
            }
            catch (NullReferenceException nullEx)
            {
                if (_options.debug != default)
                {
                    Logs?.Error(
                        @$"{_options.debug}
retry {i + 1}/{_options.RetryCount} has failed: {nullEx.Message}
{nullEx.StackTrace}");
                }

                throw nullEx;
            }
            catch (Exception ex)
            {
                if (
                    (_options.StopExceptions?.Length ?? 0) > 0 &&
                    _options.StopExceptions.Any(stopException => ex.GetType().IsAssignableTo(stopException))
                    )
                {
                    if (_options.debug != default)
                    {
                        Logs?.Warn(@$"{ex.Message}");
                    }

                    throw;
                }

                //we don't know what happened, we show the error and retry
                if (_options.debug != default)
                {
                    Logs?.Error(@$"{_options.debug}
retry {i + 1}/{_options.RetryCount} has failed: {ex.Message}");
                }
            }

            await Task.Delay(retryMilliseconds, retryCancellation ?? CancellationToken.None);

            retryCancellation?.ThrowIfCancellationRequested();

            retryMilliseconds *= (int)Math.Ceiling(_options.RetryDelayFactor);
        }

        if (_options.debug != default)
        {
            Logs?.Warn(@$"{_options.debug}
maximum retry count reached: {_options.RetryCount}");
        }

        throw new MaximumRetryCountReachedException();
    }
}