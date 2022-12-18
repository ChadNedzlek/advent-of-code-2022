using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers;
using Mono.Options;
using Spectre.Console;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string dataType = "real";
            bool menu = false;
            int puzzle = 0;
            var os = new OptionSet
            {
                { "example", v => dataType = "example" },
                { "prompt|p", v => menu = (v != null) },
                { "verbose|v", v => Helpers.IncludeVerboseOutput = (v != null) },
                { "puzzle=", v => puzzle = int.Parse(v) },
            };

            os.Parse(args);
            Dictionary<int, IProblemBase> problems = new Dictionary<int, IProblemBase>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var match = Regex.Match(type.Name, @"Problem(\d+)");
                if (match.Success)
                {
                    problems.Add(int.Parse(match.Groups[1].Value), (IProblemBase)Activator.CreateInstance(type));
                }
            }

            if (puzzle != 0)
            {
                await problems[puzzle].ExecuteAsync(dataType);
                return;
            }

            if (menu)
            {
                int problem = AnsiConsole.Prompt(
                    new SelectionPrompt<int>()
                        .Title("Which puzzle to execute?")
                        .AddChoices(problems.Keys.OrderBy(i => i)));

                await problems[problem].ExecuteAsync(dataType);
                return;
            }

            {
                var problem = problems.MaxBy(p => p.Key).Value;
                await problem.ExecuteAsync(dataType);
            }
        }
    }
}