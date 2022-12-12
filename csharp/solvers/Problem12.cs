using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem12 : ProblemBase
    {
        private record struct Coord(int Row, int Column)
        {
            public Coord Add(int r, int c)
            {
                return new Coord(Row + r, Column + c);
            }
        }

        private record class CoordPath(Coord Head, CoordPath Path)
        {
            public int Length => 1 + (Path?.Length ?? -1);
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var list = await data.ToListAsync();
            int nRows = list.Count;
            int nCols = list[0].Length;
            int[,] heights = new int[nRows, nCols];
            int [,] distance = new int[nRows, nCols];
            Coord start = default, end = default;
            
            for (var i0 = 0; i0 < heights.GetLength(0); i0++)
            for (var i1 = 0; i1 < heights.GetLength(1); i1++)
            {
                distance[i0, i1] = -1;
                char c = list[i0][i1];
                switch (c)
                {
                    case 'S':
                        start = new Coord(i0, i1);
                        c = 'a';
                        break;
                    case 'E':
                        end = new Coord(i0, i1);
                        distance[i0, i1] = 0;
                        c = 'z';
                        break;
                }
                heights[i0, i1] = c - 'a';
            }

            Queue<CoordPath> locations = new ();
            locations.Enqueue(new CoordPath(end, null));
            CoordPath bestPath = null;
            while (locations.Count > 0)
            {
                var path = locations.Dequeue();
                var head = path.Head;
                var options = new[] { head.Add(-1, 0), head.Add(1, 0), head.Add(0, -1), head.Add(0, 1) };
                foreach (var o in options)
                {
                    if (o.Row < 0 || o.Row >= nRows || o.Column < 0 || o.Column >= nCols)
                        continue;
                    var step = new CoordPath(o, path);
                    var jump = heights[head.Row, head.Column] - heights[o.Row, o.Column] ;
                    if (jump > 1)
                        continue;
                    if (distance[o.Row, o.Column] == -1 || distance[o.Row, o.Column] > step.Length)
                    {
                        distance[o.Row, o.Column] = step.Length;
                        locations.Enqueue(step);

                        if (o == start)
                        {
                            bestPath = step;
                        }
                    }
                }
            }

            for (var i0 = 0; i0 < distance.GetLength(0); i0++)
            {
                for (var i1 = 0; i1 < distance.GetLength(1); i1++)
                {
                    Helpers.Verbose($"{distance[i0, i1]:D2} ");
                }
                Helpers.VerboseLine("");
            }

            Console.WriteLine($"Best path is {bestPath.Length}");
            int trail = int.MaxValue;
            for (var i0 = 0; i0 < heights.GetLength(0); i0++)
            for (var i1 = 0; i1 < heights.GetLength(1); i1++)
            {
                int height = heights[i0, i1];
                if (height != 0)
                {
                    continue;
                }

                if (distance[i0, i1] < trail && distance[i0,i1] != -1)
                    trail = distance[i0, i1];
            }

            Console.WriteLine($"Best trail is {trail}");
        }
    }
}