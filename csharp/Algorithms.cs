using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp;

public abstract class Algorithms
{
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
    
    public static async Task<TState> BreadthFirstSearchAsync<TState, TIdentity, TScore>(TState initial,
        Func<TState, IList<TState>> nextStates,
        Func<TState, TState, bool> isBetterState,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        Func<TScore, TScore, bool> isBetterScore)
    where TScore : IEquatable<TScore>
    {
        Channel<TState> channel = Channel.CreateUnbounded<TState>();
        ConcurrentDictionary<TIdentity, TScore> loopbackDetection = new();
        TState best = initial;
        await channel.Writer.WriteAsync(initial);
        int parallelism = Environment.ProcessorCount;
        int executing = 0;
        object bestLock = new object();
        await Task.WhenAll(Enumerable.Repeat(0, parallelism).Select(_ => Task.Run(Run)));
        async Task Run() {
            while (true)
            {
                if (!await channel.Reader.WaitToReadAsync())
                {
                    return;
                }

                Interlocked.Increment(ref executing);
                if (!channel.Reader.TryRead(out var state))
                {
                    Interlocked.Decrement(ref executing);
                    continue;
                }

                if (isBetterState(state, best))
                {
                    lock (bestLock)
                    {
                        if (isBetterState(state, best))
                        {
                            best = state;
                        }
                    }
                }

                var next = nextStates(state);
                bool inserted = false;
                foreach (var n in next)
                {
                    if (getIdentity != null)
                    {
                        var stateId = getIdentity(n);
                        var score = getScore(n);
                        var addedScore = loopbackDetection.GetOrAdd(stateId, score);
                        if (!addedScore.Equals(score))
                        {
                            if (!isBetterScore(score, addedScore))
                            {
                                // We already had one, and it was already as good as or better
                                continue;
                            }

                        }
                    }

                    inserted = true;
                    await channel.Writer.WriteAsync(n);
                }

                var currentlyExecuting = Interlocked.Decrement(ref executing);

                if (!inserted)
                {
                    if (currentlyExecuting == 0)
                    {
                        if (channel.Reader.Count == 0)
                        {
                            // I'm not adding anything, there isn't anything left, and everyone else is waiting
                            channel.Writer.TryComplete();
                            return;
                        }
                    }
                }
            }
        }

        return best;
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