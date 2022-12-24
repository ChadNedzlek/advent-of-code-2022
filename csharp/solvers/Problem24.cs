using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem24 : AsyncProblemBase
    {
        public class BlizzardLocations
        {
            private readonly ImmutableList<Blizzard> _blizzards;
            private readonly Rect2<sbyte> _bounds;
            private readonly int _cycle;

            private readonly bool[][,] _cache;

            public BlizzardLocations(ImmutableList<Blizzard> blizzards, Rect2<sbyte> bounds)
            {
                _blizzards = blizzards;
                _bounds = bounds;
                _cycle = Helpers.Lcm(bounds.Right - bounds.Left + 1, bounds.Bottom - bounds.Top + 1);
                _cache = new bool[_cycle][,];
            }

            public bool IsBlizzardAt(Point2<sbyte> location, int time)
            {
                time %= _cycle;
                ref bool[,] cache = ref _cache[time];
                if (cache == null)
                {
                    lock (_cache)
                    {
                        if (cache == null)
                        {
                            cache = new bool[_bounds.Right - _bounds.Left + 1, _bounds.Bottom - _bounds.Top + 1];
                            foreach (var b in _blizzards)
                            {
                                var p = b.AtTime(_bounds, time);
                                cache[p.X - _bounds.Left, p.Y - _bounds.Top] = true;
                            }
                        }
                    }
                }

                return cache[location.X - _bounds.Left, location.Y - _bounds.Top];
            }
        }

        public record struct Blizzard(Point2<sbyte> Start, sbyte Dx, sbyte Dy)
        {
            public Point2<sbyte> AtTime(Rect2<sbyte> bounds, int time)
            {
                var res = new Point2<sbyte>(
                    (sbyte)((Start.X + (Dx * time) - bounds.Left).PosMod(bounds.Right - bounds.Left + 1) + bounds.Left),
                    (sbyte)((Start.Y + (Dy * time) - bounds.Top).PosMod(bounds.Bottom - bounds.Top + 1) + bounds.Top)
                );

                return res;
            }
        }

        public record class GameState(
            ImmutableList<Blizzard> Blizzards,
            BlizzardLocations BlizzardMap,
            Rect2<sbyte> Bounds,
            Point2<sbyte> Location,
            int Time,
            GameState Previous = null)
        {
            public bool WillBeInBlizzard(Point2<sbyte> p)
            {
                return BlizzardMap.IsBlizzardAt(p, Time + 1);
            }

            public GameState Move(Point2<sbyte> next)
            {
                if (Math.Abs(next.Y - Location.Y) + (Math.Abs(next.X - Location.X)) > 1)
                {
                    throw new ArgumentException();
                }

                return this with { Location = next, Time = Time + 1, Previous = null };
            }
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var map = await data.Select(d => d.ToArray()).ToArrayAsync();
            var blizzardBuilder = ImmutableList.CreateBuilder<Blizzard>();
            for (sbyte y = 1; y < map.Length - 1; y++)
            {
                char[] line = map[y];
                for (sbyte x = 1; x < line.Length - 1; x++)
                {
                    switch (line[x])
                    {
                        case '>':
                            blizzardBuilder.Add(new Blizzard(new Point2<sbyte>(x, y), 1, 0));
                            break;
                        case '<':
                            blizzardBuilder.Add(new Blizzard(new Point2<sbyte>(x, y), -1, 0));
                            break;
                        case '^':
                            blizzardBuilder.Add(new Blizzard(new Point2<sbyte>(x, y), 0, -1));
                            break;
                        case 'v':
                            blizzardBuilder.Add(new Blizzard(new Point2<sbyte>(x, y), 0, 1));
                            break;
                    }
                }
            }
            var bounds = new Rect2<sbyte>(1, 1, (sbyte)(map[1].Length-2), (sbyte)(map.Length-2));
            var startLocation = new Point2<sbyte>(1, 0);
            var endLocation = new Point2<sbyte>(bounds.Right, (sbyte)(bounds.Bottom + 1));
            var blizzards = blizzardBuilder.ToImmutable();
            var initialState = new GameState(blizzards, new BlizzardLocations(blizzards, bounds), bounds, startLocation, 0);
            var cycle = Helpers.Lcm(bounds.Right - bounds.Left + 1, bounds.Bottom - bounds.Top + 1);

            IEnumerable<GameState> NextStates(GameState gameState)
            {
                bool TryPoint(sbyte dx, sbyte dy, out Point2<sbyte> l)
                {
                    l = gameState.Location.Add(dx, dy);
                    if (l == endLocation || l == startLocation)
                        return true;
                    if (!bounds.IsInBounds(l))
                        return false;
                    if (gameState.WillBeInBlizzard(l))
                        return false;
                    return true;
                }

                if (TryPoint(1, 0, out var n))
                    yield return gameState.Move(n);
                if (TryPoint(-1, 0, out n))
                    yield return gameState.Move(n);
                if (TryPoint(0, 1, out n))
                    yield return gameState.Move(n);
                if (TryPoint(0, -1, out n))
                    yield return gameState.Move(n);
                if (TryPoint(0, 0, out n))
                    yield return gameState.Move(n);
                
            }
            
            void NextStatesAsync(GameState gameState, Action<GameState> returnState)
            {
                bool TryPoint(sbyte dx, sbyte dy, out Point2<sbyte> l)
                {
                    l = gameState.Location.Add(dx, dy);
                    if (l == endLocation || l == startLocation)
                        return true;
                    if (!bounds.IsInBounds(l))
                        return false;
                    if (gameState.WillBeInBlizzard(l))
                        return false;
                    return true;
                }

                if (TryPoint(1, 0, out var n))
                    returnState(gameState.Move(n));
                if (TryPoint(-1, 0, out n))
                    returnState(gameState.Move(n));
                if (TryPoint(0, 1, out n))
                    returnState(gameState.Move(n));
                if (TryPoint(0, -1, out n))
                    returnState(gameState.Move(n));
                if (TryPoint(0, 0, out n))
                    returnState(gameState.Move(n));
                
            }

            Stopwatch allWatch = Stopwatch.StartNew();
            Stopwatch pieceWatch = Stopwatch.StartNew();
            var firstPart = Algorithms.PrioritySearch(
                initialState,
                NextStates,
                s => s.Location == endLocation,
                s => s.Time - s.Location.X - s.Location.Y,
                s => (s.Location, s.Time % cycle),
                s => s.Time,
                (a, b) => a < b
            );
            Console.WriteLine($"First trip {firstPart.Time} [{pieceWatch.Elapsed}]");
            pieceWatch.Restart();
            var secondPart = Algorithms.PrioritySearch(
                firstPart,
                NextStates,
                s => s.Location == startLocation,
                s => s.Time + s.Location.X + s.Location.Y,
                s => (s.Location, s.Time % cycle),
                s => s.Time,
                (a, b) => a < b
            );
            Console.WriteLine($"Return trip {secondPart.Time} [{pieceWatch.Elapsed}]");
            var thirdPart = Algorithms.PrioritySearch(
                secondPart,
                NextStates,
                s => s.Location == endLocation,
                s => s.Time - s.Location.X - s.Location.Y,
                s => (s.Location, s.Time % cycle),
                s => s.Time,
                (a, b) => a < b
            );
            Console.WriteLine($"Final trip {thirdPart.Time}  [{pieceWatch.Elapsed} / {allWatch.Elapsed}]");
        }

        private void Render(GameState search)
        {
            if (!Helpers.IncludeVerboseOutput)
                return;

            if (search.Previous != null)
            {
                Render(search.Previous);
            }
            
            Console.SetCursorPosition(0,0);

            Console.WriteLine();
            Console.WriteLine($"Minute {search.Time}");
            for (sbyte r = search.Bounds.Top; r <= search.Bounds.Bottom; r++)
            {
                for (sbyte c = search.Bounds.Left; c <= search.Bounds.Right; c++)
                {
                    var p = new Point2<sbyte>(c, r);
                    if (search.Location == p)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("E");
                        Console.ResetColor();
                    }
                    else
                    {
                        var bs = search.Blizzards.Where(b => b.AtTime(search.Bounds, search.Time) == p).ToList();
                        var bbs = search.Blizzards.Select(b => b.AtTime(search.Bounds, search.Time)).ToArray();
                        var nb = bs.Count;
                        if (nb == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write('.');
                            Console.ResetColor();
                        }
                        else if (nb > 1)
                        {
                            Console.Write(nb);
                        }
                        else
                        {
                            switch ((bs[0].Dx, bs[0].Dy))
                            {
                                case (1, 0):
                                    Console.Write('>');
                                    break;
                                case (-1, 0):
                                    Console.Write('<');
                                    break;
                                case (0, 1):
                                    Console.Write('v');
                                    break;
                                case (0, -1):
                                    Console.Write('^');
                                    break;
                            }
                        }
                    }
                }

                Console.WriteLine();
            }
        }
    }
}