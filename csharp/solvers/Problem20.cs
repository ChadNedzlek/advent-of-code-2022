using System;
using System.Collections.Generic;
using System.Linq;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem20 : SyncProblemBase
    {
        protected override void ExecuteCore(IEnumerable<string> data)
        {
            var asList = data.ToList();
            Run(asList, 1, 1);
            Run(asList, 811589153, 10);
        }

        private static void Run(IEnumerable<string> data, int key, int iterations)
        {
            var nums = data.Select(long.Parse).Select((v, i) => (index: i, value: v * key)).ToList();
            for (int iter = 0; iter < iterations; iter++)
            for (int i = 0; i < nums.Count; i++)
            {
                Helpers.VerboseLine($"Result {string.Join(",", nums.Select(n => n.value))}");
                int which = nums.FindIndex(x => x.index == i);
                var v = nums[which];
                nums.RemoveAt(which);
                long n = (which + v.value + nums.Count) % nums.Count;
                if (n < 0)
                {
                    n += nums.Count;
                }
                nums.Insert((int)(n % nums.Count), v);
            }

            Helpers.VerboseLine($"Result {string.Join(",", nums.Select(n => n.value))}");

            int zeroIndex = nums.FindIndex(x => x.value == 0);
            long x = nums[(zeroIndex + 1000) % nums.Count].value;
            long y = nums[(zeroIndex + 2000) % nums.Count].value;
            long z = nums[(zeroIndex + 3000) % nums.Count].value;

            Console.WriteLine($"Final answer {x} + {y} + {z} = {x + y + z}");
        }
    }
}