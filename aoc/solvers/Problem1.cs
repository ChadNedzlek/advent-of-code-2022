// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem1 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            string prev = null;
            int count = 0;
            int aggIncrease = 0;
            int[] sums = { 99999,99999,99999 };
            int i = 0;
            await foreach (var item in data)
            {
                int prevSum = sums.Sum();
                sums[i % sums.Length] = int.Parse(item);
                int newSum = sums.Sum();

                if (newSum > prevSum)
                    aggIncrease++;
                if (prev != null)
                {
                    if (int.Parse(item) > int.Parse(prev))
                    {
                        count++;
                    }
                }

                prev = item;
                i++;
            }
            Console.WriteLine($"{count} increases");
            Console.WriteLine($"{aggIncrease} aggregate increases");
        }
    }
}