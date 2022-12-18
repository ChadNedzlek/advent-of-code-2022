using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem9 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            void DragTail((int x, int y) a, ref (int x, int y) b)
            {
                if (Math.Abs(a.x - b.x) <= 1 && Math.Abs(a.y - b.y) <= 1)
                    return;

                b = (b.x + Math.Sign(a.x - b.x), b.y + Math.Sign(a.y - b.y));
            }

            var positions = new (int x, int y)[10];
            HashSet<(int x, int y)> shortTail = new() { (0, 0) };
            HashSet<(int x, int y)> longTail = new() { (0, 0) };
            await foreach ((char dir, int c) in Data.As<char, int>(data, @"(.) (\d+)"))
            {
                for (int i = 0; i < c; i++)
                {
                    (int hx, int hy) = positions[0];
                    switch (dir)
                    {
                        case 'R':
                            hx++;
                            break;
                        case 'L':
                            hx--;
                            break;
                        case 'U':
                            hy--;
                            break;
                        case 'D':
                            hy++;
                            break;
                    }

                    positions[0] = (hx, hy);

                    for (int it = 0; it < 9; it++)
                    {
                        DragTail(positions[it], ref positions[it+1]);
                    }

                    shortTail.Add(positions[1]);
                    longTail.Add(positions.Last());
                }
            }

            Console.WriteLine($"Short tail went to {shortTail.Count} positions");
            Console.WriteLine($"Long tail went to {longTail.Count} positions");
        }
    }
}