using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem11 : AsyncProblemBase
    {
        public class Monkey
        {
            public int Id { get; }
            public List<long> Items { get; set; }
            public Func<long, long> Operation { get; }
            public string OpText { get; }
            public long Test { get; }
            public int TrueMonkey { get; }
            public int FalseMonkey { get; }
            public long Inspection { get; set; }

            public Monkey(int id, List<long> items, int test, int trueMonkey, int falseMonkey, Func<long, long> operation, string opText)
            {
                Id = id;
                Items = items;
                Test = test;
                TrueMonkey = trueMonkey;
                FalseMonkey = falseMonkey;
                Operation = operation;
                OpText = opText;
            }
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            await RunMonkeys(data, 20, true);
            await RunMonkeys(data, 10000, false);
        }

        private static async Task RunMonkeys(IAsyncEnumerable<string> data, int rounds, bool reduce)
        {
            List<Monkey> monkeys = new();
            foreach (var chunk in (await data.ToListAsync()).Chunk(7))
            {
                int id = Data.Parse<int>(chunk[0], @"Monkey (\d+):");
                string list = Data.Parse<string>(chunk[1], @"Starting items: ([\d, ]+)");
                var parts = list.Split(' ').Select(s => long.Parse(s.TrimEnd(','))).ToList();

                var (opName, amount) = Data.Parse<char, string>(chunk[2], @"Operation: new = old (.) (.*)");

                Func<long, long> op;
                if (amount == "old")
                {
                    op = opName switch
                    {
                        '-' => i => i - i,
                        '+' => i => i + i,
                        '*' => i => i * i,
                    };
                }
                else
                {
                    var am = long.Parse(amount);
                    op = opName switch
                    {
                        '-' => i => i - am,
                        '+' => i => i + am,
                        '*' => i => i * am,
                    };
                }

                var div = Data.Parse<int>(chunk[3], @"Test: divisible by (\d+)");
                var tMonkey = Data.Parse<int>(chunk[4], @"If true: throw to monkey (\d+)");
                var fMonkey = Data.Parse<int>(chunk[5], @"If false: throw to monkey (\d+)");

                Monkey m = new(id, parts, div, tMonkey, fMonkey, op, $"old {opName} {amount}");
                monkeys.Add(m);
            }

            var factor = monkeys.Select(m => m.Test).Aggregate((a, b) => a * b);

            for (int round = 0; round < rounds; round++)
            {
                foreach (var m in monkeys)
                {
                    var l = m.Items;
                    m.Items = new List<long>();
                    foreach (var item in l)
                    {
                        long worry = m.Operation(item);
                        m.Inspection++;
                        long reduced = worry;
                        if (reduce)
                        {
                            reduced = worry / 3;
                        }
                        reduced %= factor;
                        bool matched = reduced % m.Test == 0;
                        int target = matched ? m.TrueMonkey : m.FalseMonkey;
                        monkeys[target].Items.Add(reduced);
                        Helpers.VerboseLine(
                            $"Monkey {m.Id} inspected {item} with ({m.OpText}), resulting in {worry}, which reduced to {reduced} % {m.Test}, and {(matched ? "matched" : "NOT matched")} threw to moneky {target}");
                    }
                }

                Helpers.VerboseLine($"After round {round}:");
                foreach (var m in monkeys)
                {
                    Helpers.VerboseLine($"  Monkey {m.Id}: {string.Join(", ", m.Items)}");
                }

                Helpers.VerboseLine("");
            }

            Helpers.VerboseLine("");
            Helpers.VerboseLine("Summary");
            foreach (var m in monkeys)
            {
                Helpers.VerboseLine($"  Monkey {m.Id}: {m.Inspection}");
            }

            Helpers.VerboseLine("");

            Console.WriteLine(
                $"Monkey business = {monkeys.Select(m => m.Inspection).OrderByDescending(i => i).Take(2).Aggregate((a, b) => a * b)}");
        }
    }
}