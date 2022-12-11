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
            var os = new OptionSet
            {
                { "example", v => dataType = "example" },
                { "prompt|p", v => menu = (v != null) },
                { "verbose|v", v => Helpers.IncludeVerboseOutput = (v != null) },
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

            if (menu)
            {
                var problem = AnsiConsole.Prompt(
                    new SelectionPrompt<int>()
                        .Title("Which puzzle to execute?")
                        .AddChoices(problems.Keys.OrderBy(i => i)));

                await problems[problem].ExecuteAsync(dataType);
            }
            else{

                var problem = problems.MaxBy(p => p.Key).Value;

                await problem.ExecuteAsync(dataType);
            }
        }
    }
}