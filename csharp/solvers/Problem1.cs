// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem1 : ProblemBase, IFancyProblem
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            List<List<int>> elves = new List<List<int>>();
            List<int> current = new List<int>();
            await foreach (var num in data)
            {
                if (string.IsNullOrEmpty(num) && current.Count > 0)
                {
                    elves.Add(current);
                    current = new List<int>();
                    continue;
                }
                
                current.Add(int.Parse(num));
            }

            var sorted = elves.OrderByDescending(e => e.Sum()).ToList();
            var sums = sorted.Select(e => e.Sum()).ToList();
            Console.WriteLine($"Elf with most has {sums[0]}");
            
            Console.WriteLine($"Top 3 are {sums[0]}, {sums[1]}, and {sums[2]} = {sums.Take(3).Sum()}");
        }

        public async Task ExecuteFancyAsync(IAsyncEnumerable<string> data)
        {
            var chart = new BarChart()
                .ShowValues();
            await AnsiConsole.Live(chart).StartAsync(async ctx =>
            {
                Random r = new Random();
                BarChartItem currentItem = null;
                await foreach (var num in data)
                {
                    if (string.IsNullOrEmpty(num))
                    {
                        currentItem = null;
                        continue;
                    }

                    if (currentItem == null)
                    {
                        int c;
                        do
                        {
                            c = r.Next(1, 231);
                        } while (c is (>= 16 and <= 18));

                        currentItem = new BarChartItem(Elfo.GetName(), 0, Color.FromInt32(c));
                        chart.Data.Add(currentItem);
                    }

                    var value = int.Parse(num);
                    currentItem = new BarChartItem(currentItem.Label, currentItem.Value + value, currentItem.Color);
                    chart.Data[^1] = currentItem;
                    ctx.Refresh();
                }

                int Partition(int low, int high)
                {
                    var p = chart.Data[high].Value;
                    int i = low - 1;
                    for (int j = low; j < high; j++)
                    {
                        if (chart.Data[j].Value <= p)
                        {
                            i++;
                            (chart.Data[i], chart.Data[j]) = (chart.Data[j], chart.Data[i]);
                            ctx.Refresh();
                        }
                    }

                    i++;
                    
                    (chart.Data[i], chart.Data[high]) = (chart.Data[high], chart.Data[i]);
                    ctx.Refresh();
                    return i;
                }

                void  QuickSort(int low, int high)
                {
                    if (low >= high || low < 0)
                        return;

                    var p = Partition(low, high);

                    QuickSort(low, p - 1);
                    QuickSort(p + 1, high);
                }
                
                QuickSort(0, chart.Data.Count - 1);

                await Task.Delay(2000);

                var cc = AnsiConsole.Console;
                cc.Clear();
                cc.MarkupInterpolated($"Together [{chart.Data[^1].Color?.ToMarkup()}]{chart.Data[^1].Label}[/], [{chart.Data[^2].Color?.ToMarkup()}]{chart.Data[^2].Label}[/] and [{chart.Data[^3].Color?.ToMarkup()}]{chart.Data[^3].Label}[/], have {chart.Data[^1].Value + chart.Data[^2].Value + chart.Data[^3].Value} calories ");
            });
        }
    }
}