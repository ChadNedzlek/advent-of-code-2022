using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem16 : AsyncProblemBase
    {
        public readonly record struct NodeId(byte Value);

        public readonly record struct NodeSet(long Value)
        {
            private static long GetMask(NodeId id)
            {
                return 1L << id.Value;
            }

            public NodeSet Add(NodeId node)
            {
                return new NodeSet(Value | GetMask(node));
            }

            public NodeSet Remove(NodeId node)
            {
                return new NodeSet(Value & ~GetMask(node));
            }

            public IEnumerable<NodeId> Enumerate()
            {
                for (byte id = 0; id < 64; id++)
                {
                    if (Contains(new NodeId(id)))
                    {
                        yield return new NodeId(id);
                    }
                }
            }

            public bool Contains(NodeId node)
            {
                return (Value & GetMask(node)) != 0;
            }

            public static NodeSet From(IEnumerable<NodeId> nodes)
            {
                return new NodeSet(nodes.Aggregate(0L, (acc, n) => acc | GetMask(n)));
            }

            public static NodeSet From(params NodeId[] nodes) => From((IEnumerable<NodeId>)nodes);
            public static NodeSet Empty => new(0);

            public NodeSet Add(NodeSet other)
            {
                return new NodeSet(Value | other.Value);
            }
        }

        public class Mapping
        {
            private readonly Dictionary<string, NodeId> _mapping = new();

            public NodeId GetNodeId(string node)
            {
                if (_mapping.TryGetValue(node, out NodeId id))
                    return id;
                _mapping.Add(node, id = new NodeId((byte)_mapping.Count));
                return id;
            }

            public string Name(NodeId id)
            {
                return _mapping.First(p => p.Value == id).Key;
            }
        }

        public readonly record struct NodeDescriptor(NodeId Id, byte FlowRate, NodeSet Edges);

        public readonly record struct SingleActorScore(byte Remaining, int TotalPressure)
        {
            public bool IsBetterThan(SingleActorScore other)
            {
                if (Remaining > other.Remaining)
                    return true;

                if (TotalPressure > other.TotalPressure)
                    return true;

                return false;
            }
        }

        public readonly record struct DualActorScore(byte RemainingA, byte RemainingB, int TotalPressure)
        {
            public bool IsBetterThan(DualActorScore other)
            {
                if (RemainingA > other.RemainingA && RemainingB > other.RemainingB)
                    return true;

                if (TotalPressure > other.TotalPressure)
                    return true;

                return false;
            }
        }

        public readonly record struct SingleActorState(NodeId Location, NodeSet OpenedNodes);

        public readonly record struct ActorLocation(NodeId Node, byte Remaining);
        
        public readonly struct DualActor
        {
            public DualActor(ActorLocation a, ActorLocation b)
            {
                if (a.Node.Value > b.Node.Value)
                    (a, b) = (b, a);
                A = a;
                B = b;
            }

            public ActorLocation A { get; }
            public ActorLocation B { get; }

            public void Deconstruct(out ActorLocation a, out ActorLocation b)
            {
                a = A;
                b = B;
            }
        }

        public readonly record struct DualActorState(NodeId LocationA, NodeId LocationB, NodeSet OpenedNodes);

            public record class SingleActorPath(
            NodeId Location,
            NodeSet OpenedNodes,
            byte Remaining,
            int TotalPressure,
            SingleActorPath Previous)
        {
            public (SingleActorState State, SingleActorScore Score) GetStateAndScore()
            {
                return (new SingleActorState(Location, OpenedNodes), new SingleActorScore(Remaining, TotalPressure));
            }
        }
            
        public record class DualActorPath(
            DualActor Locations,
            NodeSet OpenedNodes,
            int TotalPressure,
            DualActorPath Previous)
        {
            public (DualActorState Locations, SingleActorScore Score) GetStateAndScore()
            {
                return (
                    new DualActorState(
                        Locations.A.Node,
                        Locations.B.Node,
                        OpenedNodes
                    ),
                    new SingleActorScore(
                        (byte)(Locations.A.Remaining + Locations.B.Remaining),
                        TotalPressure
                    )
                );
            }
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            Dictionary<NodeId, NodeDescriptor> nodes = new Dictionary<NodeId, NodeDescriptor>();
            Mapping mapping = new Mapping();
            await foreach ((string name, byte flowRate, string edges) in Data.As<string, byte, string>(data,
                               @"Valve (..) has flow rate=(\d+); tunnels? leads? to valves? (.*)"))
            {
                var nodeId = mapping.GetNodeId(name);
                var edgeList = NodeSet.From(edges.Split(",").Select(s => s.Trim()).Select(mapping.GetNodeId));
                nodes.Add(nodeId, new NodeDescriptor(nodeId, flowRate, edgeList));
            }

            // We are going to be travelling around _a lot_
            // It'll be WAY faster if we can just jump to any given node, so lets precalculate all the distances
            var distances = BuildDistanceMatrix(nodes, mapping);

            Dictionary<NodeId, byte> flow = nodes.Values
                .Where(n => n.FlowRate != 0)
                .ToDictionary(n => n.Id, n => n.FlowRate);
            Part1(mapping, distances, flow);
            var s = Stopwatch.StartNew();
            Part2Dual(mapping, distances, flow, true);
            Console.WriteLine($"Dual dead tracking: {s.Elapsed}");
            s.Restart();
            Part2(mapping, distances, flow, false);
            Console.WriteLine($"With no dead tracking: {s.Elapsed}");
            s.Restart();
            Part2(mapping, distances, flow, true);
            Console.WriteLine($"With dead tracking: {s.Elapsed}");
        }

        /// <summary>
        /// Create a compressed distance matrix, that marks the distance from any valve node to any other valve node
        /// </summary>
        /// <returns>A structure that value[fromId][toId] is the cost to travel from fromId to toId</returns>
        private static Dictionary<NodeId, Dictionary<NodeId, byte>> BuildDistanceMatrix(Dictionary<NodeId, NodeDescriptor> nodes, Mapping mapping)
        {
            Dictionary<NodeId, Dictionary<NodeId, byte>> distances = new();
            foreach (var a in nodes.Values)
            {
                distances.Add(a.Id, a.Edges.Enumerate().ToDictionary(e => e, _ => (byte)1));
            }

            bool changed;
            do
            {
                changed = false;
                foreach (var a in nodes.Values)
                {
                    foreach (var b in nodes.Values)
                    {
                        if (a.Id == b.Id)
                            continue;
                        foreach (var i in nodes.Values)
                        {
                            if (a.Id == i.Id || b.Id == i.Id)
                                continue;

                            if (!distances[a.Id].TryGetValue(b.Id, out byte aToB))
                            {
                                aToB = byte.MaxValue;
                            }

                            if (distances[a.Id].TryGetValue(i.Id, out var aToI) &&
                                distances[i.Id].TryGetValue(b.Id, out var iToB) &&
                                (aToI + iToB < aToB))
                            {
                                distances[a.Id][b.Id] = (byte)(aToI + iToB);
                                changed = true;
                            }
                        }
                    }
                }
            } while (changed);

            // We aren't going to ever go to a node with no valve, since we saved all the direct paths
            // so let's just remove all the pointless nodes from the from sections
            foreach (var empty in nodes.Values.Where(n => n.FlowRate == 0))
            {
                foreach (var other in distances.Values)
                {
                    other.Remove(empty.Id);
                }
            }
            
            // Since we aren't ever going to go TO a pointless node, we won't ever have to START their either
            // so we can remove those nodes too (though we need to keep "AA", since we start there)
            foreach (var empty in nodes.Values.Where(n => n.FlowRate == 0 && n.Id != mapping.GetNodeId("AA")))
            {
                distances.Remove(empty.Id);
            }

            return distances;
        }

        private static void Part1(Mapping mapping,
            Dictionary<NodeId, Dictionary<NodeId, byte>> distanceMatrix,
            Dictionary<NodeId, byte> flowRates)
        {
            var solve = SolveSingle(mapping.GetNodeId("AA"), 30, NodeSet.Empty, distanceMatrix, flowRates, false);
            Console.WriteLine($"Single actor relieves {solve.TotalPressure}");
        }

        private static void Part2(Mapping mapping,
            Dictionary<NodeId, Dictionary<NodeId, byte>> distanceMatrix,
            Dictionary<NodeId, byte> flowRates,
            bool trackDead)
        {
            // We are going to try every possible division of labor (which things go to the elephant and which to me)
            // And just solve each person dealing with those halves off limits (or "opened" already... by the other actor)
            // And whichever is highest is best
            // Since we can fully solve in ~5ms, doing it all 32k times shouldn't be that hard.
            var allNodeSet = flowRates.Keys.Aggregate(NodeSet.Empty, (s, n) => s.Add(n));
            var nodeList = allNodeSet.Enumerate().ToList();
            int end = 1 << nodeList.Count - 1;
            int bestPressure = 0;
            SingleActorPath a = null, b = null;
            for (int divvy = 0; divvy < end; divvy++)
            {
                NodeSet aSet = NodeSet.Empty;
                NodeSet bSet = NodeSet.Empty;
                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (((1 << i) & divvy) == 0)
                    {
                        aSet = aSet.Add(nodeList[i]);
                    }
                    else
                    {
                        bSet = bSet.Add(nodeList[i]);
                    }
                }

                var aSolve = SolveSingle(mapping.GetNodeId("AA"), 26, aSet, distanceMatrix, flowRates, trackDead);
                var bSolve = SolveSingle(mapping.GetNodeId("AA"), 26, bSet, distanceMatrix, flowRates, trackDead);
                int totalPressure = aSolve.TotalPressure + bSolve.TotalPressure;
                Helpers.VerboseLine($"  Solved with {aSolve.TotalPressure} + {bSolve.TotalPressure} = {totalPressure}");
                if (totalPressure > bestPressure)
                {
                    bestPressure = totalPressure;
                    a = aSolve;
                    b = bSolve;
                }
            }
            Console.WriteLine($"Best with {a.TotalPressure} + {b.TotalPressure} = {bestPressure}");
        }

        private static SingleActorPath SolveSingle(NodeId start,
            byte remaining,
            NodeSet opened,
            Dictionary<NodeId, Dictionary<NodeId, byte>> distanceMatrix,
            Dictionary<NodeId, byte> flowRates,
            bool trackDead)
        {
            Queue<SingleActorPath> pending = new Queue<SingleActorPath>();
            Dictionary<SingleActorState, (SingleActorScore score, SingleActorPath path)> history = new();
            HashSet<SingleActorPath> deadPaths = new();

            SingleActorPath best = new SingleActorPath(start, opened, remaining, 0, null);

            void TryEnqueue(SingleActorPath path)
            {
                if (path.Remaining <= 0)
                {
                    return;
                }

                if (best.TotalPressure < path.TotalPressure)
                {
                    best = path;
                }

                var (state, score) = path.GetStateAndScore();

                ref var ex = ref CollectionsMarshal.GetValueRefOrAddDefault(history, state, out bool found);
                if (found)
                {
                    if (!score.IsBetterThan(ex.score))
                    {
                        return;
                    }

                    if (trackDead)
                    {
                        deadPaths.Add(ex.path);
                    }
                }

                ex = (score, path);
                pending.Enqueue(path);
            }

            pending.Enqueue(best);
            
            while (pending.Count > 0)
            {
                var path = pending.Dequeue();
                if (trackDead && deadPaths.Contains(path))
                {
                    deadPaths.Remove(path);
                    continue;
                }

                foreach ((var destinationId, byte travelCost) in distanceMatrix[path.Location])
                {
                    if (path.OpenedNodes.Contains(destinationId))
                    {
                        continue;
                    }

                    if (path.Remaining <= travelCost + 1)
                    {
                        continue;
                    }

                    // Try moving to the edge
                    byte remainingAfterOpen = (byte)(path.Remaining - travelCost - 1);
                    TryEnqueue(
                        new SingleActorPath(
                            Location: destinationId,
                            Remaining: remainingAfterOpen,
                            OpenedNodes: path.OpenedNodes.Add(destinationId),
                            TotalPressure: path.TotalPressure + flowRates[destinationId] * remainingAfterOpen,
                            Previous: null // path
                        )
                    );
                }
            }

            return best;
        }
        
        private static void Part2Dual(Mapping mapping,
            Dictionary<NodeId, Dictionary<NodeId, byte>> distanceMatrix,
            Dictionary<NodeId, byte> flowRates,
            bool trackDead)
        {
            var dualSolution = SolveDual(mapping.GetNodeId("AA"), 26, NodeSet.Empty, distanceMatrix, flowRates, trackDead);
            
            Console.WriteLine($"Dual solution with {dualSolution.TotalPressure}");
        }
        
        private static DualActorPath SolveDual(NodeId start,
            byte remaining,
            NodeSet opened,
            Dictionary<NodeId, Dictionary<NodeId, byte>> distanceMatrix,
            Dictionary<NodeId, byte> flowRates,
            bool trackDead)
        {
            Queue<DualActorPath> pending = new Queue<DualActorPath>();
            Dictionary<DualActorState, (SingleActorScore score, DualActorPath path)> history = new();
            HashSet<DualActorPath> deadPaths = new();

            DualActorPath best = new DualActorPath(
                new DualActor(new ActorLocation(start, remaining), new ActorLocation(start, remaining)),
                opened,
                0,
                null
            );

            void TryEnqueue(DualActorPath path)
            {
                if (path.Locations.A.Remaining <= 0 && path.Locations.B.Remaining <= 0)
                {
                    return;
                }

                if (best.TotalPressure < path.TotalPressure)
                {
                    best = path;
                }

                var (state, score) = path.GetStateAndScore();

                ref var ex = ref CollectionsMarshal.GetValueRefOrAddDefault(history, state, out var found);
                
                if (found)
                {
                    if (ex.score.IsBetterThan(score))
                    {
                        return;
                    }
                
                    if (score.IsBetterThan(ex.score))
                    {
                        deadPaths.Add(ex.path);
                    }
                }
                
                ex = (score, path);
                pending.Enqueue(path);
            }
            
            void TryMoves(DualActorPath path, ActorLocation moving, ActorLocation other)
            {
                foreach ((var destinationId, int travelCost) in distanceMatrix[moving.Node])
                {
                    if (path.OpenedNodes.Contains(destinationId))
                    {
                        continue;
                    }

                    if (moving.Remaining <= travelCost + 1)
                    {
                        // No time to get there, do nothing.
                        continue;
                    }

                    // Try moving to the edge
                    byte remainingAfterOpen = (byte)(moving.Remaining - travelCost - 1);
                    TryEnqueue(
                        new DualActorPath(
                            Locations: new(new ActorLocation(destinationId, remainingAfterOpen), other),
                            OpenedNodes: path.OpenedNodes.Add(destinationId),
                            TotalPressure: path.TotalPressure + flowRates[destinationId] * remainingAfterOpen,
                            Previous: null //path
                        )
                    );
                }
            }

            foreach (var (aDest, aCost) in distanceMatrix[start])
            {
                foreach (var (bDest, bCost) in distanceMatrix[start])
                {
                    if (aDest.Value <= bDest.Value)
                    {
                        continue;
                    }

                    TryEnqueue(
                        new DualActorPath(
                            new DualActor(
                                new ActorLocation(aDest, (byte)(remaining - aCost - 1)),
                                new ActorLocation(bDest, (byte)(remaining - bCost - 1))
                            ),
                            NodeSet.Empty.Add(aDest).Add(bDest),
                            flowRates[aDest] * (remaining - aCost - 1) + flowRates[bDest] * (remaining - bCost - 1),
                            null
                        )
                    );
                }
            }

            while (pending.Count > 0)
            {
                var path = pending.Dequeue();
                if (trackDead && deadPaths.Contains(path))
                {
                    deadPaths.Remove(path);
                    continue;
                }
                
                TryMoves(path, path.Locations.A, path.Locations.B);
                TryMoves(path, path.Locations.B, path.Locations.A);
            }

            return best;
        }
    }
}