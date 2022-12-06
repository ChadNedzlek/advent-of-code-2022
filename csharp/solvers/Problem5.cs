﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem5 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            Dictionary<int, Stack<char>> stacks = new Dictionary<int, Stack<char>>();
            var enumerator = data.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
            {
                string line = enumerator.Current;
                if (!line.Contains('['))
                {
                    await enumerator.MoveNextAsync();
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
                        if (!stacks.TryGetValue(i, out var stack))
                        {
                            stacks.Add(i, stack = new Stack<char>());
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
            while (await enumerator.MoveNextAsync())
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