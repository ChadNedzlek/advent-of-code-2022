using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using aoc.solvers;
using Mono.Options;

namespace aoc
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string dataType = "real";
            var os = new OptionSet
            {
                { "example", v => dataType = "example" }
            };
            os.Parse(args);
            Dictionary<int, ProblemBase> problems = new Dictionary<int, ProblemBase>();
            
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var match = Regex.Match(type.Name, @"Problem(\d+)");
                if (match.Success)
                {
                    problems.Add(int.Parse(match.Groups[1].Value), (ProblemBase)Activator.CreateInstance(type));
                }
            }

            var problem = problems.OrderByDescending(p => p.Key).First().Value;

            await problem.ExecuteAsync(dataType);
        }
    }
}