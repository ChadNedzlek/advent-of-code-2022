using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp;

public abstract class Algorithms
{
    public class QueueReadySignal
    {
        private volatile TaskCompletionSource<bool> _source = new TaskCompletionSource<bool>();

        public Task<bool> WaitAsync() { return _source.Task; }

        public void Set(bool result)
        {
            if (_source.Task.IsCompleted)
                return;
            var old = Interlocked.Exchange(ref _source, new TaskCompletionSource<bool>());
            old.TrySetResult(result);
        }
    }
    
    public static TState BreadthFirstSearch<TState, TIdentity, TScore>(TState initial,
        Func<TState, IList<TState>> nextStates,
        Func<TState, TState, bool> isBetterState,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        Func<TScore, TScore, bool> isBetterScore)
    {
        Queue<TState> queue = new();
        Dictionary<TIdentity, TScore> loopbackDetection = new();
        TState best = initial;
        queue.Enqueue(initial);
        while (queue.TryDequeue(out var state))
        {
            if (isBetterState(state, best))
                best = state;

            var next = nextStates(state);

            foreach (var n in next)
            {
                if (getIdentity != null)
                {
                    var stateId = getIdentity(n);
                    ref var loopbackEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(
                        loopbackDetection,
                        stateId,
                        out bool exists
                    );
                    var score = getScore(n);
                    if (exists)
                    {
                        if (!isBetterScore(score, loopbackEntry))
                        {
                            // We already had one, and it was already as good as or better
                            continue;
                        }

                    }

                    loopbackEntry = score;
                }

                queue.Enqueue(n);
            }
        }

        return best;
    }

    public class Blist<T> : IEnumerable<T>
    {
        private int _size = 0;
        private List<T> _array;
        public int Count => _size;

        public Blist(int size)
        {
            _array = new List<T>(size);
        }

        public void Add(T item)
        {
            if (_size < _array.Count)
            {
                _array[_size++] = item;
                return;
            }

            _array.Add(item);
            _size = _array.Count;
        }

        public void Clear()
        {
            _size = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _array.Take(_size).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static async Task<TState> BreadthFirstSearchAsync<TState, TIdentity, TScore>(TState initial,
        Action<TState, Action<TState>> nextStates,
        Func<TState, TState, bool> isBetterState,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        Func<TScore, TScore, bool> isBetterScore)
    {
        const int batchSize = 2000;
        Queue<TState> queue = new();
        bool done = false;
        Dictionary<TIdentity, TScore> loopbackDetection = new(1_000_000);
        QueueReadySignal needEvent = new ();
        
        queue.Enqueue(initial);
        int parallelism = Environment.ProcessorCount;
        int executing = parallelism;
        var subResults = await Task.WhenAll(Enumerable.Repeat(0, parallelism).Select(_ => Task.Run(Run)));
        return subResults.Aggregate((a,b) => isBetterState(a,b) ? a : b);

        async Task<TState> Run()
        {
            TState best = initial;
            List<TState> batch = new (batchSize);
            List<TState> next = new (batchSize);
            while (true)
            {
                batch.Clear();
                lock (queue)
                {
                    while (batch.Count < batchSize && queue.TryDequeue(out var s))
                    {
                        batch.Add(s);
                    }
                }

                if (batch.Count == 0)
                {
                    if (done)
                        return best;

                    // There is nothing to read for some reason, we need to mark ourselves as not executing
                    var cx = Interlocked.Decrement(ref executing);

                    // If we are the last person (because the counter is zero), that means all threads are waiting
                    if (cx == 0)
                    {
                        done = true;
                        needEvent.Set(false);
                        return best;
                    }

                    // We weren't the last, wait for either more data, or the channel to close
                    if (!await needEvent.WaitAsync())
                    {
                        // The channel was closed, time to go
                        return best;
                    }

                    // We are not waiting anymore, reenter the executing state
                    Interlocked.Increment(ref executing);
                }

                next.Clear();
                foreach (var state in batch)
                {
                    if (isBetterState(state, best))
                    {
                        best = state;
                    }

                    nextStates(state, next.Add);
                }
                batch.Clear();

                if (getIdentity != null)
                {
                    var idScore = next.Select(s => (state: s, id: getIdentity(s), score: getScore(s)));
                    lock (loopbackDetection)
                    {
                        void FilterStates()
                        {
                            foreach (var (state, stateId, score) in idScore)
                            {
                                ref var loopbackEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(
                                    loopbackDetection,
                                    stateId,
                                    out bool exists
                                );
                                
                                if (exists)
                                {
                                    if (!isBetterScore(score, loopbackEntry))
                                    {
                                        // We already had one, and it was already as good as or better
                                        continue;
                                    }
                                }

                                loopbackEntry = score;
                                batch.Add(state);
                            }
                        }

                        FilterStates();
                        (next, batch) = (batch, next);
                    }
                }


                lock (queue)
                {
                    foreach (var n in next)
                    {
                        queue.Enqueue(n);
                    }
                }
            }
        }
    }
    
    public static TState BreadthFirstSearch<TState, TIdentity, TScore>(TState initial,
        Func<TState, IList<TState>> nextStates,
        IComparer<TState> stateComparer,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        IComparer<TScore> scoreComparer)
    {
        return BreadthFirstSearch(
            initial,
            nextStates,
            (a, b) => stateComparer.Compare(a, b) > 0,
            getIdentity,
            getScore,
            (a, b) => scoreComparer.Compare(a, b) > 0
        );
    }
    
    public static TState BreadthFirstSearch<TState, TIdentity, TScore>(TState initial,
        Func<TState, IList<TState>> nextStates,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore)
        where TState : IComparable<TState>
        where TScore : IComparable<TScore>
    {
        return BreadthFirstSearch(
            initial,
            nextStates,
            Comparer<TState>.Default, 
            getIdentity,
            getScore,
            Comparer<TScore>.Default
        );
    }
    
    public static TState BreadthFirstSearch<TState>(
        TState initial,
        Func<TState, IList<TState>> nextStates,
        Func<TState, TState, bool> isBetterState)
    {
        return BreadthFirstSearch<TState, int, int>(initial, nextStates, isBetterState, null, null, null);
    }
}