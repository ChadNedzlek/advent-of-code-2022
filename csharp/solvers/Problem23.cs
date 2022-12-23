using System;
using System.Collections.Generic;
using System.Linq;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem23 : SyncProblemBase
    {
        protected override void ExecuteCore(IEnumerable<string> data)
        {
            var elfData = data.Select(d => d.ToArray()).ToArray();
            Infinite2I<bool> elf = new(elfData.Length, elfData[0].Length);
            for (var r = 0; r < elfData.Length; r++)
            {
                char[] row = elfData[r];
                for (var c = 0; c < row.Length; c++)
                {
                    elf[r, c] = row[c] == '#';
                }
            }

            Infinite2I<char?> moves = new(elf.GetLength(0), elf.GetLength(1));

            char? CheckNorth(int r, int c)
            {
                if (elf[r, c] && !(elf[r - 1, c - 1] || elf[r - 1, c] || elf[r - 1, c + 1]))
                {
                    return 'N';
                }

                return null;
            }

            char? CheckSouth(int r, int c)
            {
                if (elf[r, c] && !(elf[r + 1, c - 1] || elf[r + 1, c] || elf[r + 1, c + 1]))
                {
                    return 'S';
                }

                return null;
            }

            char? CheckWest(int r, int c)
            {
                if (elf[r, c] && !(elf[r - 1, c - 1] || elf[r, c - 1] || elf[r + 1, c - 1]))
                {
                    return 'W';
                }

                return null;
            }

            char? CheckEast(int r, int c)
            {
                if (elf[r, c] && !(elf[r - 1, c + 1] || elf[r, c + 1] || elf[r + 1, c + 1]))
                {
                    return 'E';
                }

                return null;
            }

            var moveList = new List<Func<int, int, char?>> { CheckNorth, CheckSouth, CheckWest, CheckEast };

            void Render(bool showMoves)
            {
                if (!Helpers.IncludeVerboseOutput)
                    return;
                for (var r = elf.GetLowerBound(0); r <= elf.GetUpperBound(0); r++)
                {
                    for (var c = elf.GetLowerBound(1); c <= elf.GetUpperBound(1); c++)
                    {
                        Console.Write((showMoves && moves[r, c].HasValue) ? moves[r, c] : elf[r, c] ? '#' : '.');
                    }

                    Console.WriteLine();
                }
            }

            void DoRound(int i)
            {
                // Check moves
                moves.Clear();
                for (var r = elf.GetLowerBound(0); r <= elf.GetUpperBound(0); r++)
                for (var c = elf.GetLowerBound(1); c <= elf.GetUpperBound(1); c++)
                {
                    if (!elf[r, c])
                        continue;

                    int near = 0;
                    for (int ro = r - 1; ro <= r + 1; ro++)
                    for (int co = c - 1; co <= c + 1; co++)
                    {
                        if (elf[ro, co])
                            near++;
                    }

                    if (near != 1)
                    {
                        char? next = moveList.Select(f => f(r, c)).Aggregate((a, b) => a ?? b);
                        if (next.HasValue)
                        {
                            moves[r, c] = next;
                        }
                    }
                }

                Helpers.VerboseLine($"== End of round {i} ==");
                Render(true);

                for (var r = elf.GetLowerBound(0) - 1; r <= elf.GetUpperBound(0) + 1; r++)
                for (var c = elf.GetLowerBound(1) - 1; c <= elf.GetUpperBound(1) + 1; c++)
                {
                    var neighborMoves = (
                        moves[r + 1, c] == 'N',
                        moves[r - 1, c] == 'S',
                        moves[r, c + 1] == 'W',
                        moves[r, c - 1] == 'E'
                    );
                    switch (neighborMoves)
                    {
                        case (true, false, false, false):
                            elf.TrySet(r + 1, c, false);
                            elf[r, c] = true;
                            break;
                        case (false, true, false, false):
                            elf.TrySet(r - 1, c, false);
                            elf[r, c] = true;
                            break;
                        case (false, false, true, false):
                            elf.TrySet(r, c + 1, false);
                            elf[r, c] = true;
                            break;
                        case (false, false, false, true):
                            elf.TrySet(r, c - 1, false);
                            elf[r, c] = true;
                            break;
                    }
                }

                // Roll moves
                var first = moveList[0];
                moveList.RemoveAt(0);
                moveList.Add(first);
            }

            for (int i = 0; i < 10; i++)
            {
                DoRound(i);
            }
            Helpers.VerboseLine($"== Final ==");
            Render(false);

            Rect2I bounds = new Rect2I(int.MaxValue, Int32.MaxValue, int.MinValue, int.MinValue);
            
            for (var r = elf.GetLowerBound(0); r <= elf.GetUpperBound(0); r++)
            for (var c = elf.GetLowerBound(1); c <= elf.GetUpperBound(1); c++)
            {
                if (elf[r, c])
                {
                    bounds = new Rect2I(
                        Left: int.Min(bounds.Left, c),
                        Top: int.Min(bounds.Top, r),
                        Right: int.Max(bounds.Right, c),
                        Bottom: int.Max(bounds.Bottom, r)
                    );
                }
            }

            int count = 0;
            for (var r = bounds.Top; r <= bounds.Bottom; r++)
            for (var c = bounds.Left; c <= bounds.Right; c++)
            {
                if (!elf[r, c])
                {
                    count++;
                }
            }
            
            Console.WriteLine($"{count} empty spaces in {bounds}");

            for (int i = 11;; i++)
            {
                DoRound(i);
                if (!moves.Any(m => m.HasValue))
                {
                    Console.WriteLine($"After round {i}, no elf moved");
                    break;
                }
            }
        }
    }
}