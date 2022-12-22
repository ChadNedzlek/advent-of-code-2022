using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem06 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            await foreach (var line in data)
            {
                Run(line, 4);
                RunLinq(line, 4);
                Run(line, 14);
                RunLinq(line, 14);
            }
        }

        private static void Run(string line, int size)
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
        
        private static void RunLinq(string line, int size)
        {
            int headerStart = line
                .Select((_, i) => line.Substring(i, size))
                .Select(s => s.Distinct())
                .Select(s => s.Count())
                .FindIndex(count => count == size);

            int packetStart = headerStart + size;
            
            Console.WriteLine($"Found {size} marker at {packetStart} with LINQ");
        }
    }
}