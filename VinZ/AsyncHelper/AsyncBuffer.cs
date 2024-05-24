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

namespace VinZ.Common.Async;


public class AsyncBuffer<T> : IDisposable where T : class
{
    private readonly string? _debug;
    private readonly BlockingCollection<T> _queue;
    private readonly CancellationTokenSource _dispose;

    public int AddedCount { get; private set; } = 0;
    public int ConsumedCount { get; private set; } = 0;


    public AsyncBuffer(int? bufferSize = default, string? debug = default)
    {
        _debug = debug;
        _queue = new BlockingCollection<T>(bufferSize ?? Environment.ProcessorCount);
        _dispose = new CancellationTokenSource();
    }


    public void Enqueue(T item, bool complete = false)
    {
        _queue.Add(item, _dispose.Token);

        AddedCount++;

        PrintDebug("+");

        if (complete)
        {
            Complete();
        }
    }

    public void Complete()
    {
        _queue.CompleteAdding();

        PrintDebug("Complete ");
    }

    public void EnqueueMany(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Enqueue(item);
        }
    }


    public List<T> Snapshot()
    {
        var items = _queue.ToList();

        PrintDebug("=");

        return items;
    }


    public IEnumerable<T> DequeueAll()
    {
        List<T> items = new();

        while (_queue.TryTake(out var item))
        {
            items.Add(item);

            ConsumedCount++;

            PrintDebug("-");
        }

        return items;
    }


    public void Dispose()
    {
        _dispose.Cancel();

        _queue.CompleteAdding();

        DequeueAll();

        PrintDebug("Dispose ");
    }


    private void PrintDebug(string? action = default)
    {
        if (_debug != default)
        {
            Console.WriteLine($"{action ?? ""}buffer {_debug} {_queue.Count}/{_queue.BoundedCapacity}");
        }
    }




    public async IAsyncEnumerable<T> Consume([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _dispose.Token).Token;

        while (!_queue.IsAddingCompleted && !cancel.IsCancellationRequested)
        {
            T next;

            try
            {
                next = _queue.Take(cancel);

                PrintDebug();
            }
            catch (InvalidOperationException _)
            {
                // thrown when we try to Take after last item
                break;
            }

            yield return next;
        }

        Dispose();
    }


    public async Task Consume(Action<T> consume, CancellationToken cancellationToken)
    {
        PrintDebug("Begin consuming ");

        var localConsumedCount = 0;

        await foreach (var item in Consume(cancellationToken))
        {
            consume(item);

            PrintDebug($"Consumed {++localConsumedCount}");
        }

        PrintDebug("End consuming ");
    }


    public async Task Consume(Func<T, Task> consume, CancellationToken cancellationToken)
    {
        PrintDebug("Begin consuming ");

        var localConsumedCount = 0;

        await foreach (var item in Consume(cancellationToken))
        {
            await consume(item);

            PrintDebug($"Consumed {++localConsumedCount}");
        }

        PrintDebug("End consuming ");
    }
}