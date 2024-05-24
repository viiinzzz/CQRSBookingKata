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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace VinZ.Common.Async;

//https://stackoverflow.com/questions/74198179/buffering-iasyncenumerable-in-producer-consumer-scenario


public static partial class AsyncHelper
{

    //using blocking collection
    public static async IAsyncEnumerable<T> WithBuffer2<T>(
        this IAsyncEnumerable<T> source, 
        int? maxBuffer = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        string? debug = default
        )
        where T : class
    {
        
        using var queue = 
            maxBuffer == null 
                ? new BlockingCollection<T>() 
                : new BlockingCollection<T>(maxBuffer.Value);

        void PrintDebug()
        {
            if (debug != default)
            {
                Console.WriteLine($"+buffer {debug} {queue.Count}/{queue.BoundedCapacity}");
            }
        }

        var producer = Task.Run(async () => {

            await foreach (var item in source.WithCancellation(cancellationToken))//.ConfigureAwait(false))
            {
                //if (cancellationToken.IsCancellationRequested)
                //{
                //    break;
                //}
                
                queue.Add(item);

                PrintDebug();
            }

            queue.CompleteAdding();
        });

        //consumer
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            T next;
            try
            {
                next = queue.Take();
            }
            catch (InvalidOperationException _)
            {
                // thrown when we try to Take after last item
                break;
            }

            PrintDebug();

            yield return next;
        }

        // this might not be needed, task must be done 
        // if we exited the loop
        await producer.ConfigureAwait(false);
    }


    //using channel
    public static async IAsyncEnumerable<T> WithBuffer<T>(
        this IAsyncEnumerable<T> source,
        int capacity,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
    {
        ArgumentNullException.ThrowIfNull(source);

        Channel<T> channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            SingleWriter = true,
            SingleReader = true,
            //SingleWriter = false,
            //SingleReader = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        using CancellationTokenSource completionCts = new();

        Task producer = Task.Run(async () =>
        {
            try
            {
                var items = source
                    .WithCancellation(completionCts.Token)
                    .ConfigureAwait(false);

                await foreach (T item in items)
                {
                    await channel.Writer
                        .WriteAsync(item)
                        .ConfigureAwait(false);
                }
            }
            catch (ChannelClosedException)
            {
                // Ignore
            }
            finally
            {
                channel.Writer.TryComplete();
            }
        });

        try
        {
            var items = channel.Reader
                .ReadAllAsync(cancellationToken)
                .ConfigureAwait(false);

            await foreach (T item in items)
            {
                yield return item;
                cancellationToken.ThrowIfCancellationRequested();
            }

            await producer.ConfigureAwait(false); // Propagate possible source error
        }
        finally
        {
            // Prevent fire-and-forget in case the enumeration is abandoned
            if (!producer.IsCompleted)
            {
                completionCts.Cancel();
                channel.Writer.TryComplete();

                await Task.WhenAny(producer)
                    .ConfigureAwait(false);
            }
        }
    }
}