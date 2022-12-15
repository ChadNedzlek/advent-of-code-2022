using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem15 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            await Part1(data);
            await Part2(data);
        }

        private async Task Part1(IAsyncEnumerable<string> data)
        {
            Dictionary<(int x, int y), char> map = new Dictionary<(int x, int y), char>();
            await foreach (var (sx, sy, bx, by) in Data.As<int, int, int, int>(data,
                               @"Sensor at x=(-?\d+), y=(-?\d+): closest beacon is at x=(-?\d+), y=(-?\d+)"))
            {
                Set(map, bx, by, 'B');
                Set(map, sx, sy, 'S');
                int distance = Math.Abs(sx - bx) + Math.Abs(sy - by);
                for (int y = sy - distance; y <= sy + distance; y++)
                {
                    if (y != 2000000)
                    {
                        continue;
                    }

                    for (int x = sx - distance; x <= sx + distance; x++)
                    {
                        if (distance >= Math.Abs(sx - x) + Math.Abs(sy - y))
                        {
                            if (Get(map, x, y) == '.')
                                Set(map, x, y, '#');
                        }
                    }
                }
            }

            var total = For(map, 0, (a, x, y, c) => y == 2000000 && c == '#' ? a + 1 : a);
            Console.WriteLine($"Row contains {total} non-beacon spaces");
        }

        public record WeirdDiagonalBoundingBox(char Id, int Left, int Right, int Top, int Bottom);
        
        private async Task Part2(IAsyncEnumerable<string> data)
        {
            Stopwatch t = Stopwatch.StartNew();
            char boxId = 'A';

            char BoxId()
            {
                var c = ++boxId;
                if (c > 'Z')
                {
                    boxId = c = 'a';
                }
                else if (c > 'z')
                {
                    boxId = c = 'A';
                }

                return c;
            }

            var size = 8_000_000;
            List<WeirdDiagonalBoundingBox> allBoxes = new()
                { new WeirdDiagonalBoundingBox(boxId, 0, size*2, -size, size) };
            Dictionary<(int x, int y), char> map = new Dictionary<(int x, int y), char>();
            await foreach (var (sx, sy, bx, by) in Data.As<int, int, int, int>(data,
                               @"Sensor at x=(-?\d+), y=(-?\d+): closest beacon is at x=(-?\d+), y=(-?\d+)"))
            {
                
                int distance = Math.Abs(sx - bx) + Math.Abs(sy - by);
                var (sdx, sdy) = Translate(sx, sy);
                int left = sdx - distance - 1;
                int right = sdx + distance + 1;
                int top = sdy - distance - 1;
                int bottom = sdy + distance + 1;
                List<WeirdDiagonalBoundingBox> newBoxes = new();
                while (allBoxes.Count > 0)
                {
                    var slice = allBoxes[0];
                    allBoxes.RemoveAt(0);

                    if (slice.Left > right || slice.Right < left || slice.Bottom < top || slice.Top > bottom)
                    {
                        newBoxes.Add(slice);
                        continue;
                    }

                    if (right <= slice.Right)
                    {
                        newBoxes.Add(slice with { Left = right });
                    }

                    if (left >= slice.Left)
                    {
                        newBoxes.Add(slice with { Right = left, Id = BoxId() });
                    }

                    if (top >= slice.Top)
                    {
                        newBoxes.Add(new WeirdDiagonalBoundingBox(BoxId(), Math.Max(left + 1, slice.Left),
                            Math.Min(right - 1, slice.Right), slice.Top, top));
                    }

                    if (bottom <= slice.Bottom)
                    {
                        newBoxes.Add(new WeirdDiagonalBoundingBox(BoxId(), Math.Max(left + 1, slice.Left),
                            Math.Min(right - 1, slice.Right), bottom, slice.Bottom));
                    }

                    Render(allBoxes.Concat(newBoxes), 20);
                }

                newBoxes.RemoveAll(b => BoxSize(b) == 0);

                allBoxes = newBoxes;
            }

            var tinyBox = allBoxes.Where(b => BoxSize(b) == 1).ToList();
            foreach (var b in tinyBox)
            {
                var u = Untranslate(b.Left, b.Top);
                Console.WriteLine(
                    $"Single with frequency ({u.x * 4000000L + u.y}) at (x={u.x}, y={u.y}) (dx={b.Left}, dy={b.Top})");
            }
            Console.WriteLine($"Completed in {t.Elapsed}");
        }

        private static long BoxSize(WeirdDiagonalBoundingBox b)
        {
            return (b.Right - b.Left + 1) * (long)(b.Bottom - b.Top + 1);
        }

        private static void Render(IEnumerable<WeirdDiagonalBoundingBox> allBoxes, int size)
        {
            if (Helpers.IncludeVerboseOutput)
            {
                Console.WriteLine();
                Console.WriteLine("****************************************************************");
                Console.WriteLine();
                for (int y = 0; y <= size; y++)
                {
                    for (int x = 0; x <= size; x++)
                    {
                        var (dx, dy) = Translate(x, y);
                        var box = allBoxes.FirstOrDefault(b => b.Left <= dx && b.Right >= dx && b.Top <= dy && b.Bottom >= dy);
                        if (box != null)
                        {
                            Console.Write(box.Id);
                        }
                        else
                        {
                            Console.Write(".");
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        private static (int dx, int dy) Translate(int x, int y)
        {
            return(x + y, x - y);
        }
        private static (int x, int y) Untranslate(int dx, int dy)
        {
            return((dx + dy)/2, (dx - dy)/2);
        }

        private void Set(Dictionary<(int x, int y), char> dict, int x, int y, char value)
        {
            dict[(x, y)] = value;
        }

        private char Get(Dictionary<(int x, int y), char> dict, int x, int y)
        {
            if (dict.TryGetValue((x, y), out var value))
                return value;
            return '.';
        }

        private void For(Dictionary<(int x, int y), char> map, Action<int, int, char> act)
        {
            foreach (var pair in map)
            {
                act(pair.Key.x, pair.Key.y, pair.Value);
            }
        }
        
        private T For<T>(Dictionary<(int x, int y), char> map, T seed, Func<T, int, int, char, T> act)
        {
            foreach (var pair in map)
            {
                seed = act(seed, pair.Key.x, pair.Key.y, pair.Value);
            }

            return seed;
        }
    }
}