using System;
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
        Action<TState, Action<TState>> nextStates,
        Func<TState, TState, bool> isBetterState,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        Func<TScore, TScore, bool> isBetterScore)
    where TScore : IEquatable<TScore> where TState : class
    {
        Channel<TState> channel = Channel.CreateUnbounded<TState>();
        ConcurrentDictionary<TIdentity, TScore> loopbackDetection = new();
        
        await channel.Writer.WriteAsync(initial);
        int parallelism = Environment.ProcessorCount;
        int executing = parallelism;
        var subResults = await Task.WhenAll(Enumerable.Repeat(0, parallelism).Select(_ => Task.Run(Run)));
        return subResults.Aggregate((a,b) => isBetterState(a,b) ? a : b);
        
        void AddState(TState n)
        {
            if (getIdentity != null)
            {
                var stateId = getIdentity(n);
                var score = getScore(n);
                if (!loopbackDetection.TryAdd(stateId, score))
                {
                    // This will always succeed, because it's an add-only dictionary, so discard the return
                    loopbackDetection.TryGetValue(stateId, out var existingScore);
                    if (!isBetterScore(score, existingScore))
                    {
                        // We already had one, and it was already as good as or better
                        return;
                    }
                }
            }

            // Since the channel is unbounded, this will always succeed
            channel.Writer.TryWrite(n);
        }
        
        async Task<TState> Run() {
            TState best = initial;
            while (true)
            {
                TState state;
                while (!channel.Reader.TryRead(out state))
                {
                    // There is nothing to read for some reason, we need to mark ourselves as not executing
                    var cx = Interlocked.Decrement(ref executing);
                    
                    // If we are the last person (because the counter is zero), that means all threads are waiting
                    if (cx == 0)
                    {
                        Debug.Assert(channel.Reader.Count == 0);
                        channel.Writer.TryComplete();
                        return best;
                    }

                    // We weren't the last, wait for either more data, or the channel to close
                    if (!await channel.Reader.WaitToReadAsync())
                    {
                        // The channel was closed, time to go
                        return best;
                    }

                    // We are not waiting anymore, reenter the executing state
                    Interlocked.Increment(ref executing);
                }

                if (isBetterState(state, best))
                {
                    best = state;
                }

                nextStates(state, AddState);
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