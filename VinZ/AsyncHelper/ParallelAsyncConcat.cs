/*
* AsyncHelper
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

using System.Threading.Channels;
using VinZ.Common.Class;

namespace VinZ.Common.Async;


public static partial class AsyncHelper
{

   




    public static async IAsyncEnumerable<T?> AsyncConcat<T>(
        this IEnumerable<IAsyncEnumerable<T>> sources,
        CancellationToken? cancellationToken = default
    ) where T : class
    {
        foreach (var source in sources)
        {
            cancellationToken?.ThrowIfCancellationRequested();

            await foreach (var item in source)
            {
                yield return item;

                cancellationToken?.ThrowIfCancellationRequested();
            }

            yield return null; //boundary
        }
    }


    public record ParallelAsyncConcatOptions(
        int DegreeOfParallelism,
        int BufferSize,
        int WarmupSpreadMs,
        string? Debug
    ) : RecordWithValidation
    {
        public static readonly ParallelAsyncConcatOptions Default = new(
            Environment.ProcessorCount,
            0,
            50, 
            default
            );

        protected override void Validate()
        {
            if (DegreeOfParallelism < 1)
            {
                throw new ArgumentException("must be 1 or greater", nameof(DegreeOfParallelism));
            }

            if (DegreeOfParallelism > Environment.ProcessorCount)
            {
                throw new ArgumentException($"must be {Environment.ProcessorCount} or less",
                    nameof(DegreeOfParallelism));
            }

            if (BufferSize < 0)
            {
                throw new ArgumentException("must be 0 or greater", nameof(BufferSize));
            }

            if (WarmupSpreadMs < 0)
            {
                throw new ArgumentException("must be 0 or greater", nameof(WarmupSpreadMs));
            }
        }
    }

    //ETL
    public static async IAsyncEnumerable<T> ParallelAsyncConcat<E, T>(
        this IAsyncEnumerable<IAsyncEnumerable<E>> sources,
        Func<E, CancellationToken?, Task<T>> processItem,
        Action boundaryFound,
        ParallelAsyncConcatOptions options,
        CancellationToken? cancellationToken = default
    ) where E : class
    {
        var chunks = Enumerable.Range(0, options.DegreeOfParallelism)
            .Select(i => new List<IAsyncEnumerable<E>>())
            .ToArray();

        var indexedSources = sources
            .Select((source, i) => (source, i));

        await foreach (var (source, i) in indexedSources)
        {
            chunks[i % options.DegreeOfParallelism].Add(source);
        }

        var bufferSize = 
            options.BufferSize == 0 
                ? options.DegreeOfParallelism
                : options.BufferSize;

        var buffer = Channel.CreateBounded<T>(new BoundedChannelOptions(bufferSize)
        {
            SingleWriter = false,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        using var completion = new CancellationTokenSource();


        int[]? warmupMs = default;
        if (options.WarmupSpreadMs > 0)
        {
            var rand = new Random();

            warmupMs = new int[chunks.Length];
            for (int i = 0; i < chunks.Length; i++)
            {
                warmupMs[i] = (int)(rand.NextDouble() * options.WarmupSpreadMs);
            }
        }

        //processors
        var producers = chunks
            .Select(chunk => chunk.AsyncConcat(completion.Token))
            .Select((chunk, i) =>
            {
                var producer = Task.Run(async () =>
                {
                    var producerName = $"{(options.Debug == null ? "" : $"{options.Debug}-")}producer-{i}";
                    int itemCount = 0;
                    int boundaryCount = 0;
                    try
                    {
                        if (warmupMs != default)
                        {
                            await Task.Delay(warmupMs[i]);
                        }

                        //Console.WriteLine($"!{producerName} starts");
                        await foreach
                            (E? item in chunk.WithCancellation<E>(cancellationToken ?? CancellationToken.None))
                        {
                            if (item == default)
                            {
                                boundaryCount++;
                                boundaryFound?.Invoke();
                                continue;
                            }

                            var processedItem = await processItem(item, cancellationToken);

                            await buffer.Writer.WriteAsync(processedItem); //have it, write it, won't cancel here
                            itemCount++;
                            //Console.WriteLine($" +{producerName} written items: {itemCount} boundaries: {boundaryCount}");
                        }
                    }
                    catch (ChannelClosedException)
                    {
                        //ignore
                    }

                    // Console.WriteLine($"!{producerName} ends, written items: {itemCount} boundaries: {boundaryCount}");

                }, completion.Token);

                return producer;
            }).ToArray();

        var production_done = Task.WhenAll(producers).ContinueWith(_ =>
        {
            buffer.Writer.Complete();
        });

        //consumer
        var consumerName = $"{(options.Debug == null ? "" : $"{options.Debug}-")}consumer";
        int itemCount = 0;
        try
        {
            // Console.WriteLine($"!{consumerName} starts");

            var items = buffer.Reader.ReadAllAsync(cancellationToken ?? CancellationToken.None);

            await foreach (var item in items)
            {
                itemCount++;
                // Console.WriteLine($"  -{consumerName} read items: {itemCount}");
                yield return item;
                cancellationToken?.ThrowIfCancellationRequested();
                //var productionComplete = producers.All(producer => producer.IsCompleted);
                //if (productionComplete) buffer.Writer.TryComplete();
            }
        }
        finally
        {
            // Console.WriteLine($"!{consumerName} ends, read items: {itemCount}");

            if (producers.Any(producer => !producer.IsCompleted))
            {
                completion.Cancel();
                //buffer.Writer.TryComplete();

                //await Task.WhenAll(producers);
            }

            await production_done;
        }
    }
}