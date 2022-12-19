using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp;

public class Algorithms
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