using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem18 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var points = await Data.As<int, int, int>(data, @"(\d+),(\d+),(\d+)")
                .Select<(int,int,int), (int x, int y, int z)>(a => a)
                .ToHashSetAsync();
            Part1(points);
            Part2(points);
        }

        private static void Part1(HashSet<(int x, int y, int z)> points)
        {
            int total = 0;
            foreach (var p in points)
            {
                if (!points.Contains((p.x - 1, p.y, p.z)))
                    total++;
                if (!points.Contains((p.x + 1, p.y, p.z)))
                    total++;
                if (!points.Contains((p.x, p.y - 1, p.z)))
                    total++;
                if (!points.Contains((p.x, p.y + 1, p.z)))
                    total++;
                if (!points.Contains((p.x, p.y, p.z - 1)))
                    total++;
                if (!points.Contains((p.x, p.y, p.z + 1)))
                    total++;
            }

            Console.WriteLine($"Exposed any faces = {total}");
        }
        
        private static void Part2(HashSet<(int x, int y, int z)> points)
        {
            int total = 0;

            var min = points.Aggregate((a, b) => (x: int.Min(a.x, b.x), y: int.Min(a.y, b.y), z: int.Min(a.z, b.z)));
            var max = points.Aggregate((a, b) => (x: int.Max(a.x, b.x), y: int.Max(a.y, b.y), z: int.Max(a.z, b.z)));

            Dictionary<(int x, int y, int z), bool> cache = new();

            bool Exterior((int x, int y, int z) p)
            {
                if (points.Contains(p))
                    return false;

                bool? QuickCheck((int x, int y, int z) p2)
                {
                    if (p2.x < min.x || p2.y < min.y || p2.z < min.z || p2.x > max.x || p2.y > max.y || p2.z > max.z)
                        return true;

                    if (cache.TryGetValue(p2, out var result))
                        return result;

                    return null;
                }

                HashSet<(int x, int y, int z)> visited = new();

                var easyPeasy = QuickCheck(p);
                if (easyPeasy.HasValue)
                    return easyPeasy.Value;

                Queue<(int x, int y, int z)> unknown = new();

                void TryEnqueue((int x, int y, int z) q)
                {
                    if (visited.Contains(q))
                    {
                        return;
                    }

                    if (points.Contains(q))
                    {
                        return;
                    }

                    visited.Add(q);
                    unknown.Enqueue(q);
                }

                TryEnqueue((p.x - 1, p.y, p.z));
                TryEnqueue((p.x + 1, p.y, p.z));
                TryEnqueue((p.x, p.y - 1, p.z));
                TryEnqueue((p.x, p.y + 1, p.z));
                TryEnqueue((p.x, p.y, p.z - 1));
                TryEnqueue((p.x, p.y, p.z + 1));
                
                while (unknown.Count > 0)
                {
                    var check = unknown.Dequeue();

                    bool? res = QuickCheck(check);
                    if (res.HasValue)
                    {
                        cache[check] = res.Value;
                        cache[p] = res.Value;
                        return res.Value;
                    }

                    TryEnqueue((check.x - 1, check.y, check.z));
                    TryEnqueue((check.x + 1, check.y, check.z));
                    TryEnqueue((check.x, check.y - 1, check.z));
                    TryEnqueue((check.x, check.y + 1, check.z));
                    TryEnqueue((check.x, check.y, check.z - 1));
                    TryEnqueue((check.x, check.y, check.z + 1));
                }
                cache.Add(p, false);
                return false;
            }


            foreach (var p in points)
            {
                if (Exterior((p.x - 1, p.y, p.z)))
                    total++;
                if (Exterior((p.x + 1, p.y, p.z)))
                    total++;
                if (Exterior((p.x, p.y - 1, p.z)))
                    total++;
                if (Exterior((p.x, p.y + 1, p.z)))
                    total++;
                if (Exterior((p.x, p.y, p.z - 1)))
                    total++;
                if (Exterior((p.x, p.y, p.z + 1)))
                    total++;
            }

            Console.WriteLine($"Exposed exterior faces = {total}");
        }
    }
}