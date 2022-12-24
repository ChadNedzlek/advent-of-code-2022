using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Spectre.Console.Rendering;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem24 : SyncProblemBase
    {
        public record struct Blizzard(Point2I Start, int Dx, int Dy)
        {
            public Point2I AtTime(Rect2I bounds, int time)
            {
                var res = new Point2I(
                    (Start.X + (Dx * time) - bounds.Left).PosMod(bounds.Right - bounds.Left + 1) + bounds.Left,
                    (Start.Y + (Dy * time) - bounds.Top).PosMod(bounds.Bottom - bounds.Top + 1) + bounds.Top
                );

                return res;
            }
        }

        public record class GameState(
            ImmutableList<Blizzard> Blizzards,
            Rect2I Bounds,
            Point2I Location,
            int Time,
            GameState Previous = null)
        {
            public bool WillBeInBlizzard(Point2I p)
            {
                foreach (var b in Blizzards)
                {
                    if (p == b.AtTime(Bounds, Time+1))
                    {
                        return true;
                    }
                }
                return false;
            }

            public GameState Move(Point2I next)
            {
                if (Math.Abs(next.Y - Location.Y) + (Math.Abs(next.X - Location.X)) > 1)
                {
                    throw new ArgumentException();
                }

                return new GameState(Blizzards, Bounds, next, Time + 1, this);
            }
        }

        protected override void ExecuteCore(IEnumerable<string> data)
        {
            var map = data.Select(d => d.ToArray()).ToArray();
            var blizzardBuilder = ImmutableList.CreateBuilder<Blizzard>();
            for (var y = 1; y < map.Length - 1; y++)
            {
                char[] line = map[y];
                for (var x = 1; x < line.Length - 1; x++)
                {
                    switch (line[x])
                    {
                        case '>':
                            blizzardBuilder.Add(new Blizzard(new Point2I(x, y), 1, 0));
                            break;
                        case '<':
                            blizzardBuilder.Add(new Blizzard(new Point2I(x, y), -1, 0));
                            break;
                        case '^':
                            blizzardBuilder.Add(new Blizzard(new Point2I(x, y), 0, -1));
                            break;
                        case 'v':
                            blizzardBuilder.Add(new Blizzard(new Point2I(x, y), 0, 1));
                            break;
                    }
                }
            }
            var bounds = new Rect2I(1, 1, map[1].Length-2, map.Length-2);
            var startLocation = new Point2I(1, 0);
            var endLocation = new Point2I(bounds.Right, bounds.Bottom + 1);
            var initialState = new GameState(blizzardBuilder.ToImmutable(), bounds, startLocation, 0);
            var cycle = Helpers.Lcm(bounds.Right - bounds.Left + 1, bounds.Bottom - bounds.Top + 1);

            IEnumerable<GameState> NextStates(GameState gameState)
            {
                bool TryPoint(int dx, int dy, out Point2I l)
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

            var firstPart = Algorithms.PrioritySearch(
                initialState,
                NextStates,
                s => s.Location == endLocation,
                s => s.Time - s.Location.X - s.Location.Y,
                s => (s.Location, s.Time % cycle),
                s => s.Time,
                (a, b) => a < b
            );
            
            Console.WriteLine($"Initial trip {firstPart.Time}");
            var secondPart = Algorithms.PrioritySearch(
                firstPart,
                NextStates,
                s => s.Location == startLocation,
                s => s.Time + s.Location.X + s.Location.Y,
                s => (s.Location, s.Time % cycle),
                s => s.Time,
                (a, b) => a < b
            );
            Console.WriteLine($"Return trip {secondPart.Time}");
            var thirdPart = Algorithms.PrioritySearch(
                secondPart,
                NextStates,
                s => s.Location == endLocation,
                s => s.Time - s.Location.X - s.Location.Y,
                s => (s.Location, s.Time % cycle),
                s => s.Time,
                (a, b) => a < b
            );
            Console.WriteLine($"Final trip {thirdPart.Time}");
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
            for (int r = search.Bounds.Top; r <= search.Bounds.Bottom; r++)
            {
                for (int c = search.Bounds.Left; c <= search.Bounds.Right; c++)
                {
                    var p = new Point2I(c, r);
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