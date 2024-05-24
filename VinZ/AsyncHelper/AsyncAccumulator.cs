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

namespace VinZ.Common.Async;


public class AsyncAccumulator<T> where T : class
{
    public int Count { get; private set; } = 0;
    public T? Value { get; private set; } = default;

    private readonly Func<T, T, CancellationToken?, Task<T>> _aggregate;
    private readonly string? _debug;

    public AsyncAccumulator(Func<T, T, CancellationToken?, Task<T>> aggregate, string? debug = default)
    {
        _aggregate = aggregate;
        _debug = debug;
    }

    public async Task Aggregate(T item, CancellationToken? cancellationToken)
    {
        if (Value == default)
        {
            Count = 1;
            Value = item;
            return;
        }

        cancellationToken?.ThrowIfCancellationRequested();

        Count += 1;
        Value = await _aggregate(Value, item, cancellationToken);
    }

    private void PrintDebug()
    {
        if (_debug != default)
        {
            Console.WriteLine($"+accumulator {_debug} {Count}");
        }
    }


    public static async Task<AsyncAccumulator<T>> Aggregate(
        AsyncAccumulator<T> accumulator1, 
        AsyncAccumulator<T> accumulator2,
        CancellationToken? cancellationToken
        )
    {
        var aggregate = accumulator1._aggregate;

        if (accumulator2._aggregate != aggregate)
        {
            throw new ArgumentException("accumulator aggregator mismatch");
        }

        var accumulator = new AsyncAccumulator<T>(aggregate, accumulator1._debug);

        if (accumulator1.Value != null) await accumulator.Aggregate(accumulator1.Value, cancellationToken);
        if (accumulator2.Value != null) await accumulator.Aggregate(accumulator2.Value, cancellationToken);

        return accumulator;
    }

    public static async Task<T?> Aggregate(
        AsyncAccumulator<T>[] accumulators, 
        CancellationToken? cancellationToken
        )
    {
        if (accumulators.Length == 0)
        {
            return default;
        }

        if (accumulators.Length == 1)
        {
            return accumulators[0].Value;
        }

        var aggregate = accumulators[0]._aggregate;

        if (accumulators.Any(accumulator => accumulator._aggregate != aggregate))
        {
            throw new ArgumentException("accumulator aggregator mismatch");
        }

        var debug = accumulators[0]._debug;

        while (accumulators.Length > 1)
        {
            cancellationToken?.ThrowIfCancellationRequested();

            var n = (accumulators.Length + 1) / 2;

            var accumulators2 = 
                Enumerable.Range(0, n)
                    .Select(i => new AsyncAccumulator<T>(aggregate, debug))
                    .ToArray();

            await Task.WhenAll(Enumerable.Range(0, n).Select(i => Task.Run(async () =>
            {
                accumulators2[i] = await Aggregate(
                    accumulators[i * 2],
                    accumulators[i * 2 + 1],
                    cancellationToken);
            })).ToArray());
            
            accumulators = accumulators2;
        }

        return accumulators[0].Value;
    }
}