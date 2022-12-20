using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem19 : AsyncProblemBase
    {
        public record struct Resource(short Ore = 0, short Clay = 0, short Obsidian = 0, short Geode = 0) : IComparable<Resource>
        {
            public static Resource operator -(Resource a, Resource b)
            {
                return new Resource((short)(a.Ore - b.Ore), (short)(a.Clay - b.Clay), (short)(a.Obsidian - b.Obsidian), (short)(a.Geode - b.Geode));
            }

            public static Resource operator +(Resource a, Resource b)
            {
                return new Resource((short)(a.Ore + b.Ore), (short)(a.Clay + b.Clay), (short)(a.Obsidian + b.Obsidian), (short)(a.Geode + b.Geode));
            }

            public static Resource operator *(Resource a, int b)
            {
                return new Resource((short)(a.Ore * b), (short)(a.Clay * b), (short)(a.Obsidian * b), (short)(a.Geode * b));
            }
            
            public int CompareTo(Resource other)
            {
                if (Ore > other.Ore && Clay > other.Clay && Obsidian > other.Obsidian && Geode > other.Geode)
                    return 1;

                if (Ore < other.Ore && Clay < other.Clay && Obsidian < other.Obsidian && Geode < other.Geode)
                    return -1;
                
                return 0;
            }

            public static bool operator <(Resource left, Resource right)
            {
                return left.CompareTo(right) < 0;
            }

            public static bool operator >(Resource left, Resource right)
            {
                return left.CompareTo(right) > 0;
            }

            public static bool operator <=(Resource left, Resource right)
            {
                return left.Ore <= right.Ore &&
                    left.Clay <= right.Clay &&
                    left.Obsidian <= right.Obsidian &&
                    left.Geode <= right.Geode;
            }

            public static bool operator >=(Resource left, Resource right)
            {
                return left.Ore >= right.Ore &&
                    left.Clay >= right.Clay &&
                    left.Obsidian >= right.Obsidian &&
                    left.Geode >= right.Geode;
            }

            public static Resource FromOre(byte x) => new Resource(Ore: x);
            public static Resource FromClay(byte x) => new Resource(Clay: x);
            public static Resource FromObsidian(byte x) => new Resource(Obsidian: x);
            public static Resource FromGeode(byte x) => new Resource(Geode: x);
        }

        public record class Blueprint(int Id, IImmutableList<Robot> Robots);

        public readonly record struct Robot(Resource Cost, Resource Produce);

        public record FactoryState(Resource Production, Resource Stock, int Remaining);

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            List<Blueprint> blueprints = new();
            await foreach (var line in data)
            {
                int id = Data.Parse<int>(line, @"Blueprint (\d+):");
                byte oreOreCost = Data.Parse<byte>(line, @"Each ore robot costs (\d+) ore\.");
                byte clayOreCost = Data.Parse<byte>(line, @"Each clay robot costs (\d+) ore\.");
                (byte obsidianOreCost, byte obsidianClayCost) = Data.Parse<byte, byte>(line, @"Each obsidian robot costs (\d+) ore and (\d+) clay\.");
                (byte geodeOreCost, byte geodeObsidianCost) = Data.Parse<byte,byte>(line, @"Each geode robot costs (\d+) ore and (\d+) obsidian\.");
                blueprints.Add(
                    new(
                        id,
                        Robots: ImmutableList.Create(
                            new Robot(new(oreOreCost), Resource.FromOre(1)),
                            new Robot(new(clayOreCost), Resource.FromClay(1)),
                            new Robot(new(obsidianOreCost, obsidianClayCost), Resource.FromObsidian(1)),
                            new Robot(new(geodeOreCost, 0, geodeObsidianCost), Resource.FromGeode(1))
                        )
                    )
                );
            }

            long qualityLevel = 0;
            foreach (var blueprint in blueprints)
            {
                var bestState = SolveBluePrintIteratively(blueprint, 24);
            
                Console.WriteLine($"Blueprint {blueprint.Id} produced {bestState.Stock}, for a quality level of {blueprint.Id * bestState.Stock.Geode}");
                qualityLevel += blueprint.Id * bestState.Stock.Geode;
            }
            Console.WriteLine($"Quality sum = {qualityLevel}");qualityLevel = 0;
            
            foreach (var blueprint in blueprints)
            {
                var bestState = await SolveBlueprintDelegated(blueprint, 24);
            
                Console.WriteLine($"Blueprint {blueprint.Id} produced {bestState.Stock}, for a quality level of {blueprint.Id * bestState.Stock.Geode}");
                qualityLevel += blueprint.Id * bestState.Stock.Geode;
            }
            Console.WriteLine($"Quality sum = {qualityLevel}");
            
            qualityLevel = 1;
            Stopwatch bigOne = Stopwatch.StartNew();
            foreach (var blueprint in blueprints.Take(3))
            {
                Stopwatch littleOne = Stopwatch.StartNew();
                var bestState = SolveBluePrintIteratively(blueprint, 32);

                Console.WriteLine($"Blueprint {blueprint.Id} produced {bestState.Stock}, for a quality level of {blueprint.Id * bestState.Stock.Geode} [{littleOne.Elapsed}]");
                qualityLevel *= bestState.Stock.Geode;
            }
            Console.WriteLine($"Super multiple sum = {qualityLevel} [{bigOne.Elapsed}]");
            
            qualityLevel = 1;
            bigOne.Restart();
            foreach (var blueprint in blueprints.Take(3))
            {
                Stopwatch littleOne = Stopwatch.StartNew();
                var bestState = await SolveBlueprintDelegated(blueprint, 32);

                Console.WriteLine($"Blueprint {blueprint.Id} produced {bestState.Stock}, for a quality level of {blueprint.Id * bestState.Stock.Geode} [{littleOne.Elapsed}]");
                qualityLevel *= bestState.Stock.Geode;
            }
            Console.WriteLine($"Super multiple sum = {qualityLevel} [{bigOne.Elapsed}]");
        }

        private async Task<FactoryState> SolveBlueprintDelegated(Blueprint blueprint, int duration)
        {
            var maxCosts = blueprint.Robots.Select(r => r.Cost)
                .Aggregate(
                    (a, b) => new Resource(
                        short.Max(a.Ore, b.Ore),
                        short.Max(a.Clay, b.Clay),
                        short.Max(a.Obsidian, b.Obsidian),
                        short.Max(a.Geode, b.Geode)
                    )
                );

            IList<FactoryState> NextStates(FactoryState state)
            {
                ImmutableList<FactoryState> list = ImmutableList<FactoryState>.Empty;
                if (state.Remaining == 0)
                    return list;
                if (state.Remaining >= 1)
                {
                    foreach (var r in blueprint.Robots.Reverse())
                    {
                        if (state.Stock >= r.Cost)
                        {
                            if (r.Produce.Ore != 0 && maxCosts.Ore <= state.Production.Ore)
                                // We are already producing ore faster than we can ever consume it, there is no point in making more
                                continue;
                            if (r.Produce.Clay != 0 && maxCosts.Clay <= state.Production.Clay)
                                // We are already producing ore faster than we can ever consume it, there is no point in making more
                                continue;
                            if (r.Produce.Obsidian != 0 && maxCosts.Obsidian <= state.Production.Obsidian)
                                // We are already producing ore faster than we can ever consume it, there is no point in making more
                                continue;

                            var spendResources = state with { Stock = state.Stock - r.Cost };
                            var produce = StepState(spendResources);
                            var addRobot = produce with { Production = produce.Production + r.Produce };
                            list = list.Add(addRobot);

                            if (r.Produce.Geode != 0 || r.Produce.Obsidian != 0)
                            {
                                return list;
                            }
                        }
                    }
                }

                list = list.Add(StepState(state));

                return list;
            }

            return await Algorithms.BreadthFirstSearchAsync(
                new FactoryState(Resource.FromOre(1), new Resource(), duration),
                NextStates,
                (a, b) => a.Stock.Geode > b.Stock.Geode,
                s => (s.Stock, s.Production),
                s => s.Remaining,
                (a, b) => a > b
            );
        }

        private static FactoryState SolveBluePrintIteratively(Blueprint blueprint, int remaining)
        {
            Queue<FactoryState> queue = new();
            FactoryState bestState = new FactoryState(Resource.FromOre(1), new Resource(), remaining);

            Dictionary<(Resource Stock, Resource Produce), int> cache = new();

            void TryEnqueue(FactoryState s)
            {
                ref int rem = ref CollectionsMarshal.GetValueRefOrAddDefault(cache, (s.Stock, s.Production), out bool found);
                if (found && rem >= s.Remaining)
                    return;
                rem = s.Remaining;
                queue.Enqueue(s);
            }
            var maxCosts = blueprint.Robots.Select(r => r.Cost)
                .Aggregate(
                    (a, b) => new Resource(
                        short.Max(a.Ore, b.Ore),
                        short.Max(a.Clay, b.Clay),
                        short.Max(a.Obsidian, b.Obsidian),
                        short.Max(a.Geode, b.Geode)
                    )
                );

            TryEnqueue(bestState);
            while (queue.TryDequeue(out var state))
            {
                if (state.Stock.Geode > bestState.Stock.Geode)
                {
                    bestState = state;
                }

                if (state.Remaining == 0)
                {
                    continue;
                }

                void NextStates()
                {
                    if (state.Remaining >= 1)
                    {
                        foreach (var r in blueprint.Robots.Reverse())
                        {
                            if (state.Stock >= r.Cost)
                            {
                                if (r.Produce.Ore != 0 && maxCosts.Ore <= state.Production.Ore)
                                    // We are already producing ore faster than we can ever consume it, there is no point in making more
                                    continue;
                                if (r.Produce.Clay != 0 && maxCosts.Clay <= state.Production.Clay)
                                    // We are already producing ore faster than we can ever consume it, there is no point in making more
                                    continue;
                                if (r.Produce.Obsidian != 0 && maxCosts.Obsidian <= state.Production.Obsidian)
                                    // We are already producing ore faster than we can ever consume it, there is no point in making more
                                    continue;
                                
                                var spendResources = state with { Stock = state.Stock - r.Cost };
                                var produce = StepState(spendResources);
                                var addRobot = produce with { Production = produce.Production + r.Produce };
                                TryEnqueue(addRobot);

                                if (r.Produce.Geode != 0 || r.Produce.Obsidian != 0)
                                {
                                    return;
                                }
                            }
                        }
                    }

                    TryEnqueue(StepState(state));
                }

                NextStates();
            }

            return bestState;
        }

        private static FactoryState StepState(FactoryState state)
        {
            return state with{
                Stock = state.Stock + state.Production,
                Remaining = state.Remaining - 1
            };
        }
    }
}