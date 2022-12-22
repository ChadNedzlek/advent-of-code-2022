using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem05 : SyncProblemBase
    {
        protected override void ExecuteCore(IEnumerable<string> data)
        {
            Dictionary<int, Stack<char>> stacks = new Dictionary<int, Stack<char>>();
            using var enumerator = data.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string line = enumerator.Current;
                if (!line.Contains('['))
                {
                    enumerator.MoveNext();
                    break;
                }

                for (int i = 0;; i++)
                {
                    int index = i * 4 + 1;
                    if (index >= line.Length)
                        break;
                    var c = line[index];
                    if (char.IsLetter(c))
                    {
                        ref Stack<char> stack = ref CollectionsMarshal.GetValueRefOrAddDefault(stacks, i, out bool exists);
                        if (!exists)
                        {
                            stack = new Stack<char>();
                        }
                        stack.Push(c);
                    }
                }
            }

            foreach (var s in stacks.Values)
            {
                var temp = s.ToList();
                s.Clear();
                foreach(var x in temp)
                    s.Push(x);
            }

            List<string> instructions = new List<string>();
            while (enumerator.MoveNext())
            {
                instructions.Add(enumerator.Current);
            }
            
            Dictionary<int, Stack<char>> clone = stacks.ToDictionary(s => s.Key, s => new Stack<char>(s.Value.Reverse()));

            Part1(instructions, clone);
            
            clone = stacks.ToDictionary(s => s.Key, s => new Stack<char>(s.Value.Reverse()));
            Part2(instructions, clone);
        }

        private static void Part1(List<string> instructions, Dictionary<int, Stack<char>> stacks)
        {
            foreach (var line in instructions)
            {
                foreach (var s in stacks.OrderBy(s => s.Key))
                {
                    Helpers.VerboseLine($"{s.Key + 1} => {string.Join(" ", s.Value.Reverse())}");
                }

                Helpers.VerboseLine("");

                (int count, int from, int to) =
                    Data.Parse<int, int, int>(line, @"move (\d+) from (\d+) to (\d+)");
                for (int i = 0; i < count; i++)
                {
                    stacks[to - 1].Push(stacks[from - 1].Pop());
                }
            }

            var tops = stacks.OrderBy(s => s.Key).Select(s => s.Value.Peek()).ToList();

            Console.WriteLine($"Stack tops: {string.Join("", tops)}");
        }
        private static void Part2(List<string> instructions, Dictionary<int, Stack<char>> stacks)
        {
            foreach (var line in instructions)
            {
                foreach (var s in stacks.OrderBy(s => s.Key))
                {
                    Helpers.VerboseLine($"{s.Key + 1} => {string.Join(" ", s.Value.Reverse())}");
                }

                Helpers.VerboseLine("");

                (int count, int from, int to) =
                    Data.Parse<int, int, int>(line, @"move (\d+) from (\d+) to (\d+)");
                Stack<char> temp = new Stack<char>();
                for (int i = 0; i < count; i++)
                {
                    temp.Push(stacks[from-1].Pop());
                }
                for (int i = 0; i < count; i++)
                {
                    stacks[to - 1].Push(temp.Pop());
                }
                
            }

            var tops = stacks.OrderBy(s => s.Key).Select(s => s.Value.Peek()).ToList();

            Console.WriteLine($"Stack 9001 mover : {string.Join("", tops)}");
        }
    }
}