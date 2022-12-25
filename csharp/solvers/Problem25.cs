using System;
using System.Collections.Generic;
using System.Linq;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem25 : SyncProblemBase
    {
        protected override void ExecuteCore(IEnumerable<string> data)
        {
            long sum = 0;
            foreach (var line in data)
            {
                long total = SnafuToInt(line);
                Helpers.VerboseLine($"Value {total}");
                sum += total;
            }
            Console.WriteLine($"Sum is {sum}");

            string basey = IntToSnafu(sum);

            Console.WriteLine($"In base 5 = {basey}");
            Console.WriteLine($"Reversed is {SnafuToInt(basey)}");
        }

        private static string IntToSnafu(long value)
        {
            // Boring base 5 stuff
            int pow = (int)Math.Ceiling(Math.Log(value, 5));
            List<int> digits = new List<int>();
            for (int p = pow; p >= 0; p--)
            {
                int c = 0;
                long digitValue = (long)Math.Pow(5, p);
                while (value >= digitValue)
                {
                    c++;
                    value -= digitValue;
                }

                digits.Add(c);
            }

            // Now do weird "carry the 1" logic... but with negatives. Exciting
            for (int i = digits.Count - 1; i >= 1; i--)
            {
                while (digits[i] > 2)
                {
                    digits[i] -= 5;
                    digits[i - 1]++;
                }
            }

            return string.Join(
                    "",
                    digits.Select(
                        d => d switch
                        {
                            2 => '2',
                            1 => '1',
                            0 => '0',
                            -1 => '-',
                            -2 => '=',
                        }
                    )
                )
                .TrimStart('0');
        }

        private static long SnafuToInt(string line)
        {
            long total = 0;
            foreach (var c in line)
            {
                int value = c switch
                {
                    '2' => 2,
                    '1' => 1,
                    '0' => 0,
                    '-' => -1,
                    '=' => -2,
                };
                total = 5 * total + value;
            }

            return total;
        }
    }
}