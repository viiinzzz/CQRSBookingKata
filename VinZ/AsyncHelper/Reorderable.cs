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
    public interface IReorderable<T>
    {
        void RequestReorder(Func<T, int> getPriority);
        Func<T, int>? ReorderRequest { get; }
    }


    public class Reorderable<T, R> : IReorderable<T>
    {
        private readonly Func<R, Func<T, int>> _nearestTo;
        private Func<T, int>? _request;
        private DateTime? _time;

        public int ReorderRequestDelayMs { get; set; } = 10000;

        public Reorderable(Func<R, Func<T, int>> nearestTo, R? initialRequestArg = default)
        {
            _nearestTo = nearestTo;

            if (initialRequestArg == null)
            {
                return;
            }

            _request = _nearestTo(initialRequestArg);
            _time = default; //no wait
        }

        public void RequestReorder(Func<T, int> request)
        {
            _request = request;
            _time = DateTime.Now;
        }

        public Func<T, int>? ReorderRequest
        {
            get
            {
                if (_request != default
                    && (!_time.HasValue //no wait
                        || (DateTime.Now - _time.Value).TotalMilliseconds > ReorderRequestDelayMs)
                    )
                {
                    var request = _request;

                    _time = default;
                    _request = default;

                    return request;
                }

                return default;
            }
        }
    }


    public class ReorderableProducer<T>
    {
        private List<T> _sources;
        private readonly IReorderable<T> _reorderable;

        public ReorderableProducer(
            IEnumerable<T> sources,
            IReorderable<T> reorderable
        )
        {
            _sources = sources.ToList();
            _reorderable = reorderable;
        }

        public void RequestReorder(Func<T, int> getPriority) => _reorderable.RequestReorder(getPriority);

        public async Task<T?> Next()
        {
            var reorder = _reorderable.ReorderRequest;
            if (reorder != default)
            {
                await Task.Run(() =>
                {
                    _sources = _sources
                        .OrderByDescending(t => reorder(t))
                        .ToList();
                });
            }

            if (_sources.Count == 0)
            {
                return default;
            }

            var next = _sources.First();
            _sources.RemoveAt(0);

            return next;
        }

    }


    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
        this IEnumerable<T> sources, 
        IReorderable<T> reorderable,
        CancellationToken? cancellationToken = default
    )
        where T : class
    {
        var producer = new ReorderableProducer<T>(sources, reorderable);

        do
        {
            cancellationToken?.ThrowIfCancellationRequested();

            var next = await producer.Next();

            if (next == default)
            {
                yield break;
            }

            yield return next;

        } while (true);
    }

}