using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem18 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var points = await Data.AsTyped<int, int, int, IPoint3>(data, @"(\d+),(\d+),(\d+)")
                .ToHashSetAsync();
            Part1(points);
            Part2(points);
        }

        private static void Part1(HashSet<IPoint3> points)
        {
            int total = 0;
            foreach (var p in points)
            {
                if (!points.Contains((p.X - 1, p.Y, p.Z)))
                    total++;
                if (!points.Contains((p.X + 1, p.Y, p.Z)))
                    total++;
                if (!points.Contains((p.X, p.Y - 1, p.Z)))
                    total++;
                if (!points.Contains((p.X, p.Y + 1, p.Z)))
                    total++;
                if (!points.Contains((p.X, p.Y, p.Z - 1)))
                    total++;
                if (!points.Contains((p.X, p.Y, p.Z + 1)))
                    total++;
            }

            Console.WriteLine($"Exposed any faces = {total}");
        }
        
        private static void Part2(HashSet<IPoint3> points)
        {
            int total = 0;

            var min = points.Aggregate((a, b) => (x: int.Min(a.X, b.X), y: int.Min(a.Y, b.Y), z: int.Min(a.Z, b.Z)));
            var max = points.Aggregate((a, b) => (x: int.Max(a.X, b.X), y: int.Max(a.Y, b.Y), z: int.Max(a.Z, b.Z)));

            Dictionary<IPoint3, bool> cache = new();

            bool Exterior(IPoint3 p)
            {
                if (points.Contains(p))
                    return false;

                bool? QuickCheck(IPoint3 p2)
                {
                    if (p2.X < min.X || p2.Y < min.Y || p2.Z < min.Z || p2.X > max.X || p2.Y > max.Y || p2.Z > max.Z)
                        return true;

                    if (cache.TryGetValue(p2, out var result))
                        return result;

                    return null;
                }

                HashSet<IPoint3> visited = new();

                bool? easyPeasy = QuickCheck(p);
                if (easyPeasy.HasValue)
                    return easyPeasy.Value;

                Queue<IPoint3> unknown = new();

                void TryEnqueue(IPoint3 q)
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

                TryEnqueue((p.X - 1, p.Y, p.Z));
                TryEnqueue((p.X + 1, p.Y, p.Z));
                TryEnqueue((p.X, p.Y - 1, p.Z));
                TryEnqueue((p.X, p.Y + 1, p.Z));
                TryEnqueue((p.X, p.Y, p.Z - 1));
                TryEnqueue((p.X, p.Y, p.Z + 1));
                
                while (unknown.TryDequeue(out var check))
                {
                    bool? res = QuickCheck(check);
                    if (res.HasValue)
                    {
                        cache[check] = res.Value;
                        cache[p] = res.Value;
                        while (unknown.TryDequeue(out check))
                        {
                            // Anything we were curious about is on the same inside/outside, so save them too
                            cache[check] = res.Value;
                        }
                        return res.Value;
                    }

                    TryEnqueue((check.X - 1, check.Y, check.Z));
                    TryEnqueue((check.X + 1, check.Y, check.Z));
                    TryEnqueue((check.X, check.Y - 1, check.Z));
                    TryEnqueue((check.X, check.Y + 1, check.Z));
                    TryEnqueue((check.X, check.Y, check.Z - 1));
                    TryEnqueue((check.X, check.Y, check.Z + 1));
                }
                cache.Add(p, false);
                return false;
            }


            foreach (var p in points)
            {
                if (Exterior((p.X - 1, p.Y, p.Z)))
                    total++;
                if (Exterior((p.X + 1, p.Y, p.Z)))
                    total++;
                if (Exterior((p.X, p.Y - 1, p.Z)))
                    total++;
                if (Exterior((p.X, p.Y + 1, p.Z)))
                    total++;
                if (Exterior((p.X, p.Y, p.Z - 1)))
                    total++;
                if (Exterior((p.X, p.Y, p.Z + 1)))
                    total++;
            }

            Console.WriteLine($"Exposed exterior faces = {total}");
        }
    }
}