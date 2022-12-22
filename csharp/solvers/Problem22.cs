using System;
using System.Collections.Generic;
using System.Linq;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem22 : SyncProblemBase
    {
        protected override void ExecuteCore(IEnumerable<string> data)
        {
            // Part1(data);
            Part2(data);
        }
        
        protected void Part1(IEnumerable<string> data)
        {
            char[][] map = data.TakeWhile(l => !string.IsNullOrEmpty(l)).Select(l => l.ToArray()).ToArray();
            var instructions = data.Last();
            var coord = new IPoint2(Array.IndexOf(map[0], '.'), 0);
            int dx = 1, dy = 0;

            int move = 0;
            map[coord.Y][coord.X] = Display();
            RenderMap();
            foreach (var c in instructions)
            {

                switch (c)
                {
                    case 'R':
                        DoMove();
                        (dx, dy) = (-dy, dx);
                        break;
                    case 'L':
                        DoMove();
                        (dx, dy) = (dy, -dx);
                        break;
                    default:
                        move = move * 10 + (c - '0');
                        break;
                }
            }
            DoMove();
            
            Console.WriteLine($"Final location ROW={coord.Y+1}, COL={coord.X+1}, FACING={FacingValue()}, PASSWORD={(coord.Y+1) * 1000 + (coord.X+1)*4 + FacingValue()}");

            char Display()
            {
                return (dx, dy) switch
                {
                    (1, 0) => '>',
                    (-1, 0) => '<',
                    (0, 1) => 'v',
                    (0, -1) => '^',
                };
            }

            int FacingValue()
            {
                return (dx, dy) switch
                {
                    (1, 0) => 0,
                    (-1, 0) => 2,
                    (0, 1) => 1,
                    (0, -1) => 3,
                };
            }

            (int nx, int ny) FindNextStep(char[][] chars, int x, int y)
            {
                int nx = x + dx;
                int ny = y + dy;

                bool IsInMap(int xx, int yy)
                {
                    if (xx < 0 || yy < 0)
                        return false;
                    if (yy >= chars.Length)
                        return false;
                    if (xx >= chars[yy].Length)
                        return false;
                    if (chars[yy][xx] == ' ')
                        return false;
                    return true;
                }

                if (IsInMap(nx, ny))
                    return (nx, ny);

                nx -= dx;
                ny -= dy;
                while (IsInMap(nx, ny))
                {
                    nx -= dx;
                    ny -= dy;
                }

                return (nx + dx, ny + dy);
            }
            
            void DoMove()
            {
                int x = coord.X;
                int y = coord.Y;
                for (int i = 0; i < move; i++)
                {
                    (int nx, int ny) = FindNextStep(map, x, y);
                    if (map[ny][nx] != '#')
                    {
                        (x, y) = (nx, ny);
                        map[y][x] = Display();
                        RenderMap();
                    }
                    else
                    {
                        break;
                    }
                }

                coord = new IPoint2(x, y);
                move = 0;
            }

            void RenderMap()
            {
                if(!Helpers.IncludeVerboseOutput)
                    return;
                Console.SetCursorPosition(0, 0);
                foreach (var r in map)
                {
                    foreach (var c in r)
                    {
                        Console.Write(c);
                    }
                    Console.WriteLine();
                }
            }
        }
        
        protected void Part2(IEnumerable<string> data)
        {
            char[][] map = data.TakeWhile(l => !string.IsNullOrEmpty(l)).Select(l => l.ToArray()).ToArray();
            int faceSize = map.Length > 50 ? 50 : 4;
            var instructions = data.Last();
            var coord = new IPoint2(Array.IndexOf(map[0], '.'), 0);
            int dx = 1, dy = 0;

            int move = 0;
            map[coord.Y][coord.X] = Display();
            
            foreach (var c in instructions)
            {
                switch (c)
                {
                    case 'R':
                        DoMove();
                        (dx, dy) = (-dy, dx);
                        break;
                    case 'L':
                        DoMove();
                        (dx, dy) = (dy, -dx);
                        break;
                    default:
                        move = move * 10 + (c - '0');
                        break;
                }
            }
            DoMove();
            
            RenderMap();
            
            Console.WriteLine($"Final location ROW={coord.Y+1}, COL={coord.X+1}, FACING={FacingValue()}, PASSWORD={(coord.Y+1) * 1000 + (coord.X+1)*4 + FacingValue()}");

            char Display()
            {
                return (dx, dy) switch
                {
                    (1, 0) => '>',
                    (-1, 0) => '<',
                    (0, 1) => 'v',
                    (0, -1) => '^',
                };
            }

            int FacingValue()
            {
                return (dx, dy) switch
                {
                    (1, 0) => 0,
                    (-1, 0) => 2,
                    (0, 1) => 1,
                    (0, -1) => 3,
                };
            }

            static (int nx, int ny, int dx, int dy) FindNextStep(char[][] chars, int x, int y, int ddx, int ddy)
            {
                int nx = x + ddx;
                int ny = y + ddy;
                (int dx, int dy) = (ddx, ddy);
                const int faceSize = 50;

                bool IsInMap(int xx, int yy)
                {
                    if (xx < 0 || yy < 0)
                        return false;
                    if (yy >= chars.Length)
                        return false;
                    if (xx >= chars[yy].Length)
                        return false;
                    if (chars[yy][xx] == ' ')
                        return false;
                    return true;
                }

                if (IsInMap(nx, ny))
                    return (nx, ny, dx, dy);

                if (faceSize == 50)
                {
                    switch ((x / 50, y / 50, dx, dy))
                    {
                        // Face A
                        case (1, 0, -1, 0):
                            Helpers.VerboseLine("Went from A -> E");
                            (dx, dy) = (1, 0);
                            ny = 3 * faceSize - y - 1;
                            nx = 0;
                            break;
                        case (1, 0, 0, -1):
                            Helpers.VerboseLine("Went from A -> F");
                            (dx, dy) = (1, 0);
                            ny = x + 2 * faceSize;
                            nx = 0;
                            break;
                        // Face B
                        case (2,0,1,0):
                            Helpers.VerboseLine("Went from B -> D");
                            (dx, dy) = (-1, 0);
                            nx = faceSize * 2 - 1;
                            ny = faceSize * 3 - y - 1;
                            break;
                        case (2,0,0,-1):
                            Helpers.VerboseLine("Went from B -> F");
                            nx = x - 2 * faceSize;
                            ny = faceSize * 4 - 1;
                            break;
                        case (2,0,0,1):
                            Helpers.VerboseLine("Went from B -> C");
                            (dx, dy) = (-1, 0);
                            ny = x - faceSize;
                            nx = faceSize * 2 - 1;
                            break;
                        // Face C
                        case (1, 1, 1, 0):
                            Helpers.VerboseLine("Went from C -> B");
                            (dx, dy) = (0, -1);
                            ny = faceSize - 1;
                            nx = faceSize + y;
                            break;
                        case (1, 1, -1, 0):
                            Helpers.VerboseLine("Went from C -> E");
                            (dx, dy) = (0, 1);
                            nx = y - faceSize;
                            ny = 2 * faceSize;
                            break;
                        // Face D
                        case(1,2,1,0):
                            Helpers.VerboseLine("Went from D -> B");
                            (dx, dy) = (-1, 0);
                            nx = faceSize * 3 - 1;
                            ny = 3 * faceSize - y - 1;
                            break;
                        case(1,2,0,1):
                            Helpers.VerboseLine("Went from D -> F");
                            (dx, dy) = (-1, 0);
                            nx = faceSize - 1;
                            ny = x + 2 * faceSize;
                            break;
                        // Face E
                        case(0,2,-1,0):
                            Helpers.VerboseLine("Went from E -> A");
                            (dx, dy) = (1, 0);
                            nx = faceSize;
                            ny = 3 * faceSize - y - 1;
                            break;
                        case(0,2,0,-1):
                            Helpers.VerboseLine("Went from E -> C");
                            (dx, dy) = (1, 0);
                            nx = faceSize;
                            ny = x + faceSize;
                            break;
                        // Face F
                        case(0,3,-1,0):
                            Helpers.VerboseLine("Went from F -> A");
                            (dx, dy) = (0, 1);
                            nx = y - 2 * faceSize;
                            ny = 0;
                            break;
                        case(0,3,1,0):
                            Helpers.VerboseLine("Went from F -> D");
                            (dx, dy) = (0, -1);
                            nx = y - 2 * faceSize;
                            ny = 3 * faceSize - 1;
                            break;
                        case(0,3,0,1):
                            Helpers.VerboseLine("Went from F -> B");
                            nx = x + 2 * faceSize;
                            ny = 0;
                            break;
                    }
                }
                return (nx, ny, dx, dy);
            }
            
            void DoMove()
            {
                int x = coord.X;
                int y = coord.Y;
                map[y][x] = Display();
                for (int i = 0; i < move; i++)
                {
                    (int nx, int ny, int ndx, int ndy) = FindNextStep(map, x, y, dx, dy);
                    if (map[ny][nx] != '#')
                    {
                        (x, y) = (nx, ny);
                        (dx, dy) = (ndx, ndy);
                        map[y][x] = Display();
                    }
                    else
                    {
                        break;
                    }
                }

                coord = new IPoint2(x, y);
                //RenderMap();
                move = 0;
            }

            void RenderMap()
            {
                if(!Helpers.IncludeVerboseOutput)
                    return;
                Console.SetCursorPosition(0, 0);
                foreach (var r in map)
                {
                    foreach (var c in r)
                    {
                        switch (c)
                        {
                            case '<':
                            case '>':
                            case 'v':
                            case '^':
                                Console.ForegroundColor = ConsoleColor.Green;
                                break;
                        }
                        Console.Write(c);
                        Console.ResetColor();
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}