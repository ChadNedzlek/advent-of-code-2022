using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem16 : ProblemBase
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

        public readonly record struct NodeDescriptor(NodeId Id, int FlowRate, NodeSet Edges);

        public readonly record struct Score(int Remaining, int TotalPressure)
        {
            public bool IsBetterThan(Score other)
            {
                if (Remaining > other.Remaining)
                    return true;

                if (TotalPressure > other.TotalPressure)
                    return true;

                return false;
            }
        }

        public readonly record struct ActorState(NodeId Location, NodeSet OpenedNodes);

        public record class EasyPath(
            NodeId Location,
            NodeSet OpenedNodes,
            int Remaining,
            int TotalPressure,
            EasyPath Previous)
        {
            public (ActorState State, Score Score) GetStateAndScore()
            {
                return (new ActorState(Location, OpenedNodes), new Score(Remaining, TotalPressure));
            }
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            Dictionary<NodeId, NodeDescriptor> nodes = new Dictionary<NodeId, NodeDescriptor>();
            Mapping mapping = new Mapping();
            await foreach ((string name, int flowRate, string edges) in Data.As<string, int, string>(data,
                               @"Valve (..) has flow rate=(\d+); tunnels? leads? to valves? (.*)"))
            {
                var nodeId = mapping.GetNodeId(name);
                var edgeList = NodeSet.From(edges.Split(",").Select(s => s.Trim()).Select(mapping.GetNodeId));
                nodes.Add(nodeId, new NodeDescriptor(nodeId, flowRate, edgeList));
            }

            // We are going to be travelling around _a lot_
            // It'll be WAY faster if we can just jump to any given node, so lets precalculate all the distances
            var distances = BuildDistanceMatrix(nodes, mapping);

            Dictionary<NodeId, int> flow = nodes.Values
                .Where(n => n.FlowRate != 0)
                .ToDictionary(n => n.Id, n => n.FlowRate);
            Part1(mapping, distances, flow);
            Part2(mapping, distances, flow);
        }

        /// <summary>
        /// Create a compressed distance matrix, that marks the distance from any valve node to any other valve node
        /// </summary>
        /// <returns>A structure that value[fromId][toId] is the cost to travel from fromId to toId</returns>
        private static Dictionary<NodeId, Dictionary<NodeId, int>> BuildDistanceMatrix(Dictionary<NodeId, NodeDescriptor> nodes, Mapping mapping)
        {
            Dictionary<NodeId, Dictionary<NodeId, int>> distances = new();
            foreach (var a in nodes.Values)
            {
                distances.Add(a.Id, a.Edges.Enumerate().ToDictionary(e => e, _ => 1));
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

                            if (!distances[a.Id].TryGetValue(b.Id, out int aToB))
                            {
                                aToB = int.MaxValue;
                            }

                            if (distances[a.Id].TryGetValue(i.Id, out var aToI) &&
                                distances[i.Id].TryGetValue(b.Id, out var iToB) &&
                                (aToI + iToB < aToB))
                            {
                                distances[a.Id][b.Id] = aToI + iToB;
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
            Dictionary<NodeId, Dictionary<NodeId, int>> distanceMatrix,
            Dictionary<NodeId, int> flowRates)
        {
            var solve = Solve(mapping.GetNodeId("AA"), 30, NodeSet.Empty, distanceMatrix, flowRates);
            Console.WriteLine($"Single actor relieves {solve.TotalPressure}");
        }

        private static void Part2(Mapping mapping,
            Dictionary<NodeId, Dictionary<NodeId, int>> distanceMatrix,
            Dictionary<NodeId, int> flowRates)
        {
            // We are going to try every possible division of labor (which things go to the elephant and which to me)
            // And just solve each person dealing with those halves off limits (or "opened" already... by the other actor)
            // And whichever is highest is best
            // Since we can fully solve in ~5ms, doing it all 32k times shouldn't be that hard.
            var allNodeSet = flowRates.Keys.Aggregate(NodeSet.Empty, (s, n) => s.Add(n));
            var nodeList = allNodeSet.Enumerate().ToList();
            int end = 1 << nodeList.Count;
            int bestPressure = 0;
            EasyPath a = null, b = null;
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

                var aSolve = Solve(mapping.GetNodeId("AA"), 26, aSet, distanceMatrix, flowRates);
                var bSolve = Solve(mapping.GetNodeId("AA"), 26, bSet, distanceMatrix, flowRates);
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

        private static EasyPath Solve(NodeId start,
            int remaining,
            NodeSet opened,
            Dictionary<NodeId, Dictionary<NodeId, int>> distanceMatrix,
            Dictionary<NodeId, int> flowRates)
        {
            Queue<EasyPath> pending = new Queue<EasyPath>();
            Dictionary<ActorState, Score> history = new();

            EasyPath best = new EasyPath(start, opened, remaining, 0, null);

            void TryEnqueue(EasyPath path)
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

                if (history.TryGetValue(state, out var xScore) && !score.IsBetterThan(xScore))
                {
                    return;
                }

                history[state] = score;
                pending.Enqueue(path);
            }

            pending.Enqueue(best);
            while (pending.Count > 0)
            {
                var path = pending.Dequeue();
                foreach ((var destinationId, int travelCost) in distanceMatrix[path.Location])
                {
                    if (path.OpenedNodes.Contains(destinationId))
                    {
                        continue;
                    }

                    // Try moving to the edge
                    int remainingAfterOpen = path.Remaining - travelCost - 1;
                    TryEnqueue(
                        new EasyPath(
                            Location: destinationId,
                            Remaining: remainingAfterOpen,
                            OpenedNodes: path.OpenedNodes.Add(destinationId),
                            TotalPressure: path.TotalPressure + flowRates[destinationId] * remainingAfterOpen,
                            Previous: path
                        )
                    );
                }
            }

            return best;
        }
    }
}