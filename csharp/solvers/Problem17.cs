using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem17 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var line = await data.FirstAsync();
            List<bool[]> map = new();
            string[] rockStrings = {
                """
                ####
                """,
                
                """
                .#.
                ###
                .#.
                """,
                
                """
                ..#
                ..#
                ###
                """,
                
                """
                #
                #
                #
                #
                """,
                
                """
                ##
                ##
                """};

            List<bool[,]> rocks = rockStrings.Select(TranslateRock).ToList();

            //await RunExecution(map, rocks, line, 2022);
            await RunExecution(map, rocks, line, 1000000000000);
        }

        private class Signature
        {
            private int[] Heights { get; }
            private int RockIndex { get; }
            private int JetIndex { get; }

            public Signature(int[] heights, int rockIndex, int jetIndex)
            {
                Heights = heights;
                RockIndex = rockIndex;
                JetIndex = jetIndex;
            }

            protected bool Equals(Signature other)
            {
                return
                    RockIndex == other.RockIndex &&
                    JetIndex == other.JetIndex &&
                    Heights.SequenceEqual(other.Heights);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Signature)obj);
            }

            public override int GetHashCode()
            {
                HashCode code = new HashCode();
                foreach (var h in Heights)
                    code.Add(h);
                code.Add(RockIndex);
                code.Add(JetIndex);
                return code.ToHashCode();
            }
        }

        private async Task RunExecution(List<bool[]> map, List<bool[,]> rocks, string line, long iterations)
        {
            int iJet = 0;
            long lost = 0;
            Dictionary<Signature, (long iter, long height)> repeats = new();
            for (long iRock = 0; iRock < iterations; iRock++)
            {
                if (repeats != null)
                {
                    var curHeight = map.FindLastIndex(row => row.Any(s => s)) + 1 + lost;
                    var sig = new Signature(GetSignature(map), (int)(iRock % rocks.Count), iJet % line.Length);
                    if (repeats.TryGetValue(sig, out var rep))
                    {
                        Console.WriteLine($"We saw the same signature at iteration {rep.iter} and {iRock}, fast forwarding...");
                        long mod = iRock - rep.iter;
                        long hMod = curHeight - rep.height;
                        long fakeCycles = (iterations - iRock) / mod;
                        Console.WriteLine($"The height then was {rep.height} the current height is {curHeight}");
                        curHeight = map.FindLastIndex(row => row.Any(s => s)) + 1 + lost;
                        iRock += fakeCycles * mod;
                        lost += fakeCycles * hMod;
                        Console.WriteLine($"After {fakeCycles} iterations it is iteration {iRock} the height is {curHeight}");

                        repeats = null;
                    }
                    else
                    {
                        repeats.Add(sig, (iRock, curHeight));
                    }
                }

                if (iRock % 100_000 == 0)
                    Console.WriteLine($"Iteration {iRock}");
                lost += TrimMap(map);
                var top = map.FindLastIndex(row => row.Any(s => s));
                if (top == -1) top = map.Count - 1;
                var rock = rocks[(int)(iRock % rocks.Count)];
                while (map.Count < top + 4 + rock.GetLength(0)) map.Add(new bool[7]);
                var left = 2;
                var bottom = top + 4;

                if (Helpers.IncludeVerboseOutput)
                {
                    Overlay(map, rock, bottom, left);
                    RenderMap(map);
                    DeOverlay(map, rock, bottom, left);
                }

                while (true)
                {
                    int tryLeft = left +
                                  line[iJet % line.Length] switch
                                  {
                                      '<' => -1,
                                      '>' => 1
                                  };
                    iJet++;
                    if (TryOverlay(map, rock, bottom, tryLeft))
                    {
                        left = tryLeft;
                    }

                    int tryBottom = bottom - 1;
                    if (TryOverlay(map, rock, tryBottom, left))
                    {
                        bottom = tryBottom;
                    }
                    else
                    {
                        Overlay(map, rock, bottom, left);
                        RenderMap(map);
                        break;
                    }

                    if (Helpers.IncludeVerboseOutput)
                    {
                        Overlay(map, rock, bottom, left);
                        RenderMap(map);
                        DeOverlay(map, rock, bottom, left);
                        await Task.Delay(100);
                    }
                }
            }

            var height = map.FindLastIndex(row => row.Any(s => s)) + 1 + lost;
            Console.WriteLine($"Height is {height}");
        }

        private int[] GetSignature(List<bool[]> map)
        {
            if (map.Count == 0)
                return new int[7];
            
            int[] tops = new int[map[0].Length];
            for (var r = map.Count - 1; r >= 0; r--)
            {
                for (var c = 0; c < tops.Length; c++)
                {
                    if (map[r][c] && tops[c] == 0)
                        tops[c] = r;
                }
            }
            return tops;
        }
        private int TrimMap(List<bool[]> map)
        {
            if (map.Count == 0)
                return 0;
            
            int[] tops = new int[map[0].Length];
            for (var r = map.Count - 1; r >= 0; r--)
            {
                for (var c = 0; c < tops.Length; c++)
                {
                    if (map[r][c] && tops[c] == 0)
                        tops[c] = r;
                }
            }

            var edge = tops.Min();
            if (edge > 0)
            {
                map.RemoveRange(0, edge);
            }

            return edge;
        }

        private void RenderMap(List<bool[]> map)
        {
            if (!Helpers.IncludeVerboseOutput)
                return;
            Console.Clear();
            foreach (bool[] line in Enumerable.Reverse(map))
            {

                foreach (var b in line)
                {
                    Console.Write(b ? "#" : ".");
                }

                Console.WriteLine();
            }
        }

        private void Overlay(List<bool[]> map, bool[,] rock, int bottom, int left)
        {
            for (var i0 = 0; i0 < rock.GetLength(0); i0++)
            for (var i1 = 0; i1 < rock.GetLength(1); i1++)
            {
                if (!rock[i0, i1])
                    continue;
                map[i0 + bottom][i1 + left] = true;
            }
        }
        private void DeOverlay(List<bool[]> map, bool[,] rock, int bottom, int left)
        {
            for (var i0 = 0; i0 < rock.GetLength(0); i0++)
            for (var i1 = 0; i1 < rock.GetLength(1); i1++)
            {
                if (!rock[i0, i1])
                    continue;
                map[i0 + bottom][i1 + left] = false;
            }
        }

        private bool TryOverlay(List<bool[]> map, bool[,] rock, int bottom, int left)
        {
            if (left < 0)
                return false;
            if (left > map[0].Length - rock.GetLength(1))
                return false;
            if (bottom < 0)
                return false;
            for (var i0 = 0; i0 < rock.GetLength(0); i0++)
            for (var i1 = 0; i1 < rock.GetLength(1); i1++)
            {
                if (!rock[i0, i1])
                    continue;
                if (map[i0 + bottom][i1 + left])
                    return false;
            }

            return true;
        }

        private bool[,] TranslateRock(string pattern)
        {
            var lines = pattern.Split('\n').Select(p => p.TrimEnd()).Reverse().ToList();
            var rock = new bool[lines.Count, lines[0].Length];
            for (int r = 0; r < lines.Count; r++)
            {
                for (int c = 0; c < lines[r].Length; c++)
                {
                    rock[r, c] = lines[r][c] == '#';
                }
            }

            return rock;
        }
    }
}