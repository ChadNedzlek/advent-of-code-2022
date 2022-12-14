using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem02 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            int basic = 0;
            int complicated = 0;
            await foreach ((char them, char me) in Data.As<char, char>(data, "(.) (.)"))
            {
                basic += me switch
                {
                    'X' => 1 + them switch { 'A' => 3, 'B' => 0, 'C' => 6, },
                    'Y' => 2 + them switch { 'A' => 6, 'B' => 3, 'C' => 0, },
                    'Z' => 3 + them switch { 'A' => 0, 'B' => 6, 'C' => 3, },
                };
                complicated += me switch
                {
                    'X' => 0 + them switch { 'A' => 3, 'B' => 1, 'C' => 2, },
                    'Y' => 3 + them switch { 'A' => 1, 'B' => 2, 'C' => 3, },
                    'Z' => 6 + them switch { 'A' => 2, 'B' => 3, 'C' => 1, },
                };
            }
            
            Console.WriteLine($"Easy plan score: {basic}");
            Console.WriteLine($"Complicated plan score: {complicated}");
        }
    }
}