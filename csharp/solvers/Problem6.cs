// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem6 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            await foreach (var line in data)
            {
                Run(line, 4);
                Run(line, 14);
            }
        }

        private void Run(string line, int size)
        {
            Dictionary<char, int> counts = new();
            for (var i = 0; i < line.Length; i++)
            {
                char c = line[i];
                counts.Increment(c);
                if (i >= size)
                {
                    counts.Decrement(line[i-size]);
                }

                if (counts.Values.Count(v => v == 1) == size)
                {
                    Console.WriteLine($"Found {size} marker at {i+1}");
                    return;
                }
            }
        }
    }
}