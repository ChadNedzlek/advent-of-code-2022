using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem15 : AsyncProblemBase
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

        public record WeirdDiagonalRect(char Id, int Left, int Right, int Top, int Bottom);
        
        private async Task Part2(IAsyncEnumerable<string> data)
        {
            // We are going to do this whole thing rotated 45 degrees, and we are going to call those
            // "diagonal coordinates" (or dx,dy).
            // dx = x + y, dy = x - y
            // This turns all the areas where mines might be into squares, and we can talk about places that we know there
            // aren't beacons as "WeirdDiagonalRect", and then we don't need to actually look at each space,
            // we can just talk about the hypotehtical ranges themselves.
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
            
            // To start with we need a rect that can cover the original x/y range of 0-size
            List<WeirdDiagonalRect> potentialBeaconAreas = new() { new WeirdDiagonalRect(boxId, 0, size*2, -size, size) };
            Dictionary<(int x, int y), char> map = new Dictionary<(int x, int y), char>();
            await foreach (var (sx, sy, bx, by) in Data.As<int, int, int, int>(data,
                               @"Sensor at x=(-?\d+), y=(-?\d+): closest beacon is at x=(-?\d+), y=(-?\d+)"))
            {
                
                int distance = Math.Abs(sx - bx) + Math.Abs(sy - by);
                var (sdx, sdy) = ToDiagonalCoordinates(sx, sy);
                int left = sdx - distance - 1;
                int right = sdx + distance + 1;
                int top = sdy - distance - 1;
                int bottom = sdy + distance + 1;
                List<WeirdDiagonalRect> newBoxes = new();
                while (potentialBeaconAreas.Count > 0)
                {
                    // We need to check each potential beacon area and "slice out" the box of (left,right,top,bottom)
                    var toSlice = potentialBeaconAreas[0];
                    potentialBeaconAreas.RemoveAt(0);

                    if (toSlice.Left > right || toSlice.Right < left || toSlice.Bottom < top || toSlice.Top > bottom)
                    {
                        newBoxes.Add(toSlice);
                        continue;
                    }

                    // Rip off the sides first
                    // So if 'a' is the old box, and '.' is what we are removing, we slice off l and r
                    // aaaaaa      llaarr
                    // aaaaaa      llaarr
                    // aa..aa  ==> ll..rr
                    // aa..aa  ==> ll..rr
                    // aaaaaa      llaarr
                    // aaaaaa      llaarr
                    if (right <= toSlice.Right)
                    {
                        newBoxes.Add(toSlice with { Left = right });
                    }

                    if (left >= toSlice.Left)
                    {
                        newBoxes.Add(toSlice with { Right = left, Id = BoxId() });
                    }

                    // There are still some top bottom pieces, we need to slice off t and b
                    // aa      tt
                    // aa      tt
                    // ..  ==> ..
                    // ..  ==> ..
                    // aa      bb
                    // aa      bb
                    if (top >= toSlice.Top)
                    {
                        newBoxes.Add(new WeirdDiagonalRect(BoxId(), Math.Max(left + 1, toSlice.Left),
                            Math.Min(right - 1, toSlice.Right), toSlice.Top, top));
                    }

                    if (bottom <= toSlice.Bottom)
                    {
                        newBoxes.Add(new WeirdDiagonalRect(BoxId(), Math.Max(left + 1, toSlice.Left),
                            Math.Min(right - 1, toSlice.Right), bottom, toSlice.Bottom));
                    }
                    
                    // We've created new boxes l, r, t, and b, and that accounts for all previous a, and we are ditching
                    // Z, so we've managed to create up to 4 new boxes, and accounted for all the squares

                    Render(potentialBeaconAreas.Concat(newBoxes), 20);
                }

                // We might have sliced away all the box pieces, so remove any empty ones
                newBoxes.RemoveAll(b => BoxSize(b) <= 0);

                potentialBeaconAreas = newBoxes;
            }

            var tinyBox = potentialBeaconAreas.Where(b => BoxSize(b) == 1).ToList();
            foreach (var b in tinyBox)
            {
                var u = FromDiagonalCoordinates(b.Left, b.Top);
                Console.WriteLine(
                    $"Single with frequency ({u.x * 4000000L + u.y}) at (x={u.x}, y={u.y}) (dx={b.Left}, dy={b.Top})");
            }
            Console.WriteLine($"Completed in {t.Elapsed}");
        }

        private static long BoxSize(WeirdDiagonalRect b)
        {
            return (b.Right - b.Left + 1) * (long)(b.Bottom - b.Top + 1);
        }

        private static void Render(IEnumerable<WeirdDiagonalRect> allBoxes, int size)
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
                        var (dx, dy) = ToDiagonalCoordinates(x, y);
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

        private static (int dx, int dy) ToDiagonalCoordinates(int x, int y)
        {
            return(x + y, x - y);
        }
        
        private static (int x, int y) FromDiagonalCoordinates(int dx, int dy)
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