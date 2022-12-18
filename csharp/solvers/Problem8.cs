using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem8 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var list = await data.ToListAsync();
            int[,] height = new int[list.Count, list[0].Length];
            for (var r = 0; r < list.Count; r++)
            {
                string row = list[r];
                for (var c = 0; c < row.Length; c++)
                {
                    height[r, c] = int.Parse(row[c].ToString());
                }
            }

            int nRows = height.GetLength(0);
            int nCols = height.GetLength(1);
            bool[,] visible = new bool[nRows, nCols];
            int[,] score = new int[nRows, nCols];

            var cons = AnsiConsole.Console;

            for (var r = 0; r < nRows; r++)
            for (var c = 0; c < nCols; c++)
            {
                int x = height[r, c];
                visible[r, c] =
                    !(0..r).AsEnumerable().Any(i => height[i, c] >= x) ||
                    !((r + 1)..nRows).AsEnumerable().Any(i => height[i, c] >= x) ||
                    !(0..c).AsEnumerable().Any(i => height[r, i] >= x) ||
                    !((c + 1)..nCols).AsEnumerable().Any(i => height[r, i] >= x);

                int up = 0;
                for (int i = r - 1; i >= 0; i--)
                {
                    up++;
                    if (height[i, c] >= height[r, c])
                        break;
                }

                int left = 0;
                for (int i = c - 1; i >= 0; i--)
                {
                    left++;
                    if (height[r, i] >= height[r, c])
                        break;
                }

                int down = 0;
                for (int i = r + 1; i < nRows; i++)
                {
                    down++;
                    if (height[i, c] >= height[r, c])
                        break;
                }

                int right = 0;
                for (int i = c + 1; i < nCols; i++)
                {
                    right++;
                    if (height[r, i] >= height[r, c])
                        break;
                }

                score[r, c] = up * down * left * right;
            }

            Helpers.IfVerbose(() =>
            {
                for (var r = 0; r < nRows; r++)
                {
                    for (var c = 0; c < nCols; c++)
                    {
                        int x = height[r, c];
                        if (visible[r, c])
                        {
                            cons.Write(x.ToString(), new Style(Color.Green));
                        }
                        else
                        {
                            cons.Write(x.ToString(), new Style(Color.Red));
                        }
                    }

                    cons.WriteLine();
                }
            });

            int count = visible.Cast<bool>().Count(v => v);

            Console.WriteLine($"{count} trees visible");
            Console.WriteLine($"Best scenic score is {score.AsEnumerable().Max()}");
        }
    }
}