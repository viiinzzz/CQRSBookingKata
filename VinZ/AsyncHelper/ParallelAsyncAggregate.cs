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

public static partial class AsyncHelper
{
    public static async Task<T?> ParallelAsyncAggregate<T>(
        this IAsyncEnumerable<T> source,
        Func<T, T, CancellationToken?,Task<T>> aggregate,
        CancellationToken? cancellationToken = default,
        string? debug = default
    ) where T : class
    {
        var accumulators =
            Enumerable.Range(0, Environment.ProcessorCount)
                .Select(i => new AsyncAccumulator<T>(aggregate, debug))
                .ToArray();

        int i = 0;
        await foreach (var item in source)
        {
            cancellationToken?.ThrowIfCancellationRequested();

            accumulators[i = (i + 1) % Environment.ProcessorCount]
                .Aggregate(item, cancellationToken);
        }

        return await AsyncAccumulator<T>.Aggregate(accumulators, cancellationToken);
    }
}