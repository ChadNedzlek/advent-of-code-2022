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
    public class Problem4 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var count = 0;
            var anyOver = 0;
            await foreach ((int aStart, int aEnd, int bStart, int bEnd) in Data.As<int, int, int, int>(data, @"(\d+)-(\d+),(\d+)-(\d+)"))
            {
                var a = Enumerable.Range(aStart, aEnd - aStart + 1).ToHashSet();
                var b = Enumerable.Range(bStart, bEnd - bStart + 1).ToHashSet();
                int overlapCount = a.Intersect(b).Count();
                if (overlapCount == a.Count || overlapCount == b.Count)
                {
                    count++;
                }

                if (overlapCount != 0)
                {
                    anyOver++;
                }
            }
            Console.WriteLine($"Count of total overlaps = {count}");
            Console.WriteLine($"Count of partial overlaps = {anyOver}");

            var linq = await ExecuteLinq(data);
            Console.WriteLine($"LINQ match total = {linq.total == count}");
            Console.WriteLine($"LINQ match partial = {linq.partial == anyOver}");
        }

        private async Task<(int total, int partial)> ExecuteLinq(IAsyncEnumerable<string> data)
        {
            var overlaps = await Data.As<int, int, int, int>(data, @"(\d+)-(\d+),(\d+)-(\d+)")
                .Select(t => (a: t.Item1..(t.Item2 + 1), b: t.Item3..(t.Item4 + 1))).Select(p => (
                    a: p.a.GetOffsetAndLength(int.MaxValue).Length,
                    b: p.b.GetOffsetAndLength(int.MaxValue).Length,
                    overlap: p.a.AsEnumerable().Intersect(p.b.AsEnumerable()).Count()))
                .ToListAsync();

            int total = overlaps.Count(p => p.overlap == p.a || p.overlap == p.b);
            int any = overlaps.Count(p => p.overlap != 0);

            return (total, any);
        }
    }
}