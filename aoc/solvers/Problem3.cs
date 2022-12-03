// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Spectre.Console;

namespace aoc.solvers
{
    public class Problem3 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var group = new List<HashSet<char>>();
            var compartmentOverlap = 0;
            var badgeOverlap = 0;
            await foreach (var line in data)
            {
                {
                    HashSet<char> a = new HashSet<char>(line[..(line.Length / 2)]);
                    HashSet<char> b = new HashSet<char>(line[(line.Length / 2)..]);
                    var overlap = a.Intersect(b).Single();
                    var priority = CalculatePriority(overlap);
                    Console.WriteLine($"Overlap is {overlap} with priority {priority}");
                    compartmentOverlap += priority;
                }

                {
                    group.Add(new HashSet<char>(line));
                    if (group.Count != 3) continue;
                    
                    var overlap = group[0].Intersect(group[1]).Intersect(group[2]).Single();
                    group.Clear();
                    var priority = CalculatePriority(overlap);
                    Console.WriteLine($"Badge {overlap} with priority {priority}");
                    badgeOverlap += priority;
                }
            }

            Console.WriteLine($"Rucksack priority: {compartmentOverlap}");
            Console.WriteLine($"Badge priority: {badgeOverlap}");
        }

        private static int CalculatePriority(char overlap)
        {
            return (overlap <= 'Z') ? overlap - 'A' + 27 : overlap - 'a' + 1;
        }
    }
}