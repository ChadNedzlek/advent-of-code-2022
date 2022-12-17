using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
                    if(Contains(new NodeId(id)))
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
        }

        public readonly struct TwoNodes
        {
            public TwoNodes(NodeId a, NodeId b)
            {
                if (a.Value > b.Value)
                    (b, a) = (a, b);
                A = a;
                B = b;
            }

            public NodeId A { get; }
            public NodeId B { get; }

            public void Deconstruct(out NodeId a, out NodeId b)
            {
                a = A;
                b = B;
            }

            public bool Equals(TwoNodes other)
            {
                return A.Equals(other.A) && B.Equals(other.B);
            }

            public override bool Equals(object obj)
            {
                return obj is TwoNodes other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(A, B);
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

        public readonly record struct HardState(TwoNodes ActorLocations, NodeSet OpenedNodes);

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
        
        public record class HardPath(
            TwoNodes Actors,
            NodeSet OpenedNodes,
            int Available,
            int Remaining,
            int TotalPressure,
            HardPath Previous)
        {
            public (HardState State, Score Score) GetStateAndScore()
            {
                return (new HardState(Actors, OpenedNodes), new Score(Remaining, TotalPressure));
            }
        }

        public readonly record struct EasyState(NodeId Location, NodeSet OpenedNodes);
        public record class EasyPath(
            NodeId Location,
            NodeSet OpenedNodes,
            NodeSet Visited,
            int Remaining,
            int TotalPressure,
            EasyPath Previous)
        {
            public (EasyState State, Score Score) GetStateAndScore()
            {
                return (new EasyState(Location, OpenedNodes), new Score(Remaining, TotalPressure));
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

                            int aToB;
                            if (!distances[a.Id].TryGetValue(b.Id, out aToB))
                            {
                                aToB = int.MaxValue;
                            }

                            if (distances[a.Id].TryGetValue(i.Id, out var aToI) &&
                                distances[i.Id].TryGetValue(b.Id, out var iToB) &&
                                (aToI + iToB < aToB))
                            {
                                distances[a.Id][b.Id] = aToB;
                                changed = true;
                            }
                        }
                    }
                }
            } while (changed);

            Part1(mapping, nodes);
        }

        public class Inverted : IComparer<int>
        {
            private readonly Comparer<int> _comparer  = Comparer<int>.Default;

            public int Compare(int x, int y)
            {
                return _comparer.Compare(y, x);
            }
        }

        public record struct GraphNode(NodeId Id, ImmutableDictionary<NodeId, int> Edges);

        private static void Part1(Mapping mapping, Dictionary<NodeId, NodeDescriptor> nodes)
        {
            EasyPath GetNextStep(NodeId start, int remaining, NodeSet opened)
            {
                Queue<EasyPath> pending = new Queue<EasyPath>();
                Dictionary<EasyState, Score> history = new();

                EasyPath best = new EasyPath(start, opened, new NodeSet().Add(start), remaining, 0, null);
                
                void TryEnqueue(EasyPath path)
                {
                    if (path.Remaining == 0)
                    {
                        return;
                    }

                    if (best.TotalPressure < path.TotalPressure)
                    {
                        best = path;
                    }

                    var (state, score) = path.GetStateAndScore();

                    if (history.TryGetValue(state, out var xScore) && ! score.IsBetterThan(xScore))
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
                    var desc = nodes[path.Location];
                    if (!path.OpenedNodes.Contains(path.Location) && desc.FlowRate != 0)
                    {
                        // Try opening this one
                        TryEnqueue(path with
                        {
                            Remaining = path.Remaining - 1,
                            OpenedNodes = path.OpenedNodes.Add(path.Location),
                            TotalPressure = path.TotalPressure + desc.FlowRate * (path.Remaining - 1),
                            Visited = new NodeSet().Add(path.Location),
                            Previous = path,
                        });
                    }

                    foreach (var edge in desc.Edges.Enumerate())
                    {
                        if (path.Visited.Contains(edge))
                        {
                            continue;
                        }
                        
                        // Try moving to the edge
                        TryEnqueue(path with{
                            Location = edge,
                            Remaining = path.Remaining - 1,
                            Visited = path.Visited.Add(edge),
                            Previous = path.Previous,
                        });
                    }
                }

                return best;
            }

            NodeSet o = NodeSet.Empty;
            int duration = 26;
            int count = 2;
            var aa = mapping.GetNodeId("AA");
            PriorityQueue<(NodeId location, string name), int> position = new (new Inverted());
            //position.Enqueue((aa, "elephant"), 26);
            for (int i = 0; i < count; i++)
            {
                position.Enqueue((aa, "agent " + (i+1)), duration);
            }

            int totalPressure = 0;
            while (position.TryDequeue(out var entry, out int remaining))
            {
                var next = GetNextStep(entry.location, remaining, o);
                if (next.TotalPressure == 0)
                    continue;
                var first = next;
                while (first.Previous.Previous != null) first = first.Previous;
                var prev = first.Previous;
                Helpers.VerboseLine($"{entry.name} goes to {mapping.Name(prev.Location)} and relieves {first.TotalPressure} at {duration - prev.Remaining + 1}");
                totalPressure += first.TotalPressure;
                o = o.Add(prev.Location);
                //DumpPath(next);
                position.Enqueue((prev.Location, entry.name), prev.Remaining - 1);
            }

            Console.WriteLine($"Total pressure relieved is {totalPressure}");

            void DumpPath(EasyPath path)
            {
                if (path.Previous != null)
                    DumpPath(path.Previous);

                Console.WriteLine(
                    $"{duration - path.Remaining} opens {mapping.Name(path.Location)}, opened nodes {string.Join(", ", path.OpenedNodes.Enumerate().Select(mapping.Name))} with {path.TotalPressure}");
            }
        }
    }
}