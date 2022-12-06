// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem3 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            (int compartmentOverlap, int badgeOverlap) = await CompartmentOverlap(data);
            Console.WriteLine($"Rucksack priority: {compartmentOverlap}");
            Console.WriteLine($"Badge priority: {badgeOverlap}");
            Console.WriteLine();

            var linq = WithLinq(await data.ToListAsync());
            var query = WithLinq(await data.ToListAsync());
            
            Console.WriteLine($"Compartment with LINQ check: {compartmentOverlap == linq.compartmentOverlap}");
            Console.WriteLine($"Badge with LINQ check: {badgeOverlap == linq.badgeOverlap}");
            Console.WriteLine($"Compartment with query syntax check: {compartmentOverlap == query.compartmentOverlap}");
            Console.WriteLine($"Badge with query syntax check: {badgeOverlap == query.badgeOverlap}");
        }

        private static async Task<(int compartmentOverlap, int badgeOverlap)> CompartmentOverlap(IAsyncEnumerable<string> data)
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
                    Helpers.VerboseLine($"Overlap is {overlap} with priority {priority}");
                    compartmentOverlap += priority;
                }

                {
                    group.Add(new HashSet<char>(line));
                    if (group.Count != 3) continue;

                    var overlap = group[0].Intersect(group[1]).Intersect(group[2]).Single();
                    group.Clear();
                    var priority = CalculatePriority(overlap);
                    Helpers.VerboseLine($"Badge {overlap} with priority {priority}");
                    badgeOverlap += priority;
                }
            }
            
            Helpers.VerboseLine("");

            return (compartmentOverlap, badgeOverlap);
        }

        private static int CalculatePriority(char overlap)
        {
            return (overlap <= 'Z') ? overlap - 'A' + 27 : overlap - 'a' + 1;
        }

        private static (int compartmentOverlap, int badgeOverlap) WithLinq(IEnumerable<string> data)
        {
            var part1 = data
                .Select(d => d[..(d.Length / 2)]
                    .Intersect(d[(d.Length / 2)..])
                    .Select(CalculatePriority)
                    .Single())
                .Sum();
            
            var part2 = data
                .Chunk<IEnumerable<char>>(3)
                .Select(elves => elves.Aggregate((a, b) => a.Intersect(b)))
                .Select(overlap => overlap.Single())
                .Select(CalculatePriority)
                .Sum();

            return (part1, part2);
        }
        
        private static (int compartmentOverlap, int badgeOverlap) WithQuerySyntax(IEnumerable<string> data)
        {
            var part1 = from line in data
                let a = line[..(line.Length / 2)]
                let b = line[(line.Length / 2)..]
                let overlap = a.Intersect(b).Single()
                select CalculatePriority(overlap);
            var part2 = from values in data.Chunk<IEnumerable<char>>(3)
                let overlap = values.Aggregate((a, b) => a.Intersect(b)).Single()
                select CalculatePriority(overlap);
            return (part1.Sum(), part2.Sum());
        }
    }
}