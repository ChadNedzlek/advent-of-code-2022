using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem14 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            int buffer = 300;
            var asList = await data.ToListAsync();
            int minX = int.MaxValue, minY = 0, maxX = 0, maxY = 0;
            foreach(var line in asList)
            {
                var parts = line
                    .Split('>')
                    .Select(s => s.Trim('-', ' '))
                    .Select(s => s
                        .Split(',')
                        .Select(int.Parse)
                        .ToArray()
                    );
                foreach (var point in parts)
                {
                    minX = Math.Min(minX, point[0]);
                    minY = Math.Min(minY, point[1]);
                    maxX = Math.Max(maxX, point[0]);
                    maxY = Math.Max(maxY, point[1]);
                }
            }
            
            char [,] board = (char[,])Array.CreateInstance(typeof(char), new[] { maxX - minX + 2*buffer, maxY - minY + 2*buffer },
                new[] { minX - buffer, minY - buffer });
            foreach(var line in asList)
            {
                var parts = line
                    .Split('>')
                    .Select(s => s.Trim('-', ' '))
                    .Select(s => s
                        .Split(',')
                        .Select(int.Parse)
                        .ToArray()
                    )
                    .Select(a => (x:a[0], y:a[1]))
                    .ToArray();
                for (var i = 1; i < parts.Length; i++)
                {
                    var pos = parts[i - 1];
                    var end = parts[i];
                    while (pos != end)
                    {
                        board[pos.x, pos.y] = '#';
                        pos = (pos.x + Math.Sign(end.x - pos.x), pos.y + Math.Sign(end.y - pos.y));
                    }
                    board[pos.x, pos.y] = '#';
                }

                board[500, 0] = '+';
            }

            DrawBoard(board);
            for (var index0 = board.GetLowerBound(0); index0 <= board.GetUpperBound(0); index0++)
            {
                board[index0, maxY + 2] = '#';
            }


            int count = 0;
            while (true)
            {
                int x = 500, y = 0;
                while (true)
                {
                    if (y == maxY + 3)
                    {
                        // Lost time to go
                        goto endLoop;
                    }

                    try
                    {
                        if (board[x, y + 1] == 0)
                        {
                            y++;
                            continue;
                        }

                        if (board[x - 1, y + 1] == 0)
                        {
                            x--;
                            y++;
                            continue;
                        }

                        if (board[x + 1, y + 1] == 0)
                        {
                            x++;
                            y++;
                            continue;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.SetCursorPosition(0,0);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Helpers.IncludeVerboseOutput = true;
                        DrawBoard(board);
                        return;
                    }

                    board[x, y] = '@';
                    Console.SetCursorPosition(0,0);
                    board[x, y] = 'o';
                    DrawBoard(board);
                    count++;
                    if (y == 0)
                    {
                        goto endLoop;
                    }

                    break;
                }
            }
            endLoop:
            Helpers.VerboseLine(""); 
            Helpers.VerboseLine("After");
            Helpers.VerboseLine("");
            DrawBoard(board);
            
            Console.WriteLine($"Dropped {count} sand");
        }

        private static void DrawBoard(char[,] board)
        {
            if (!Helpers.IncludeVerboseOutput)
                return;
            for (var index0 = board.GetLowerBound(0); index0 <= board.GetUpperBound(0); index0++)
            {
                for (var index1 = board.GetLowerBound(1); index1 <= board.GetUpperBound(1); index1++)
                {
                    Helpers.Verbose(board[index0, index1].ToString());
                }

                Helpers.VerboseLine("");
            }
        }
    }
}