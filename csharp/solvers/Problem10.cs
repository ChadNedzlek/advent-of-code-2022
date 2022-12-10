using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem10 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            await RunSequential(data);
            await RunRender(data);
        }

        private class ExecutionState
        {
            public Dictionary<string, int> Registers { get; } = new()
            {
                { "X", 1 }
            };

            public override string ToString()
            {
                StringBuilder b = new StringBuilder();
                foreach (var (name, value) in Registers.OrderBy(r => r.Key))
                {
                    b.Append($"{name} = {value}");
                }

                return b.ToString();
            }
        }

        private abstract class Instruction
        {
            public int Delay { get; private set; }

            public Instruction(int delay)
            {
                Delay = delay;
            }

            public bool TryExecute(ExecutionState state)
            {
                Delay--;
                if (Delay > 0)
                {
                    return false;
                }

                Execute(state);
                return true;
            }

            protected abstract void Execute(ExecutionState state);
        }

        private class DelayInstruction : Instruction
        {
            public DelayInstruction() : base(1)
            {
            }

            protected override void Execute(ExecutionState state)
            {
            }

            public override string ToString() => "noop";
        }

        private class AddXInstruction : Instruction
        {
            private readonly int _amount;

            public AddXInstruction(int amount) : base(2)
            {
                _amount = amount;
            }

            protected override void Execute(ExecutionState state)
            {
                state.Registers["X"] += _amount;
            }
            public override string ToString() => $"addx {_amount}";
        }

        private async Task RunParallel(IAsyncEnumerable<string> data)
        {
            int ip = 0;
            ExecutionState state = new();
            List<Instruction> pending = new List<Instruction>();
            await foreach (var line in data)
            {
                ip++;

                Helpers.VerboseLine($"At ip {ip} value is {state.Registers["X"]}");
                
                if (string.IsNullOrEmpty(line))
                    break;

                pending.Add(ParseInstruction(line));

                List<Instruction> next = new List<Instruction>();
                foreach (var ins in pending)
                {
                    if (!ins.TryExecute(state))
                    {
                        next.Add(ins);
                    }
                }

                pending = next;
            }

            while (pending.Count > 0)
            {
                ip++;
                Helpers.VerboseLine($"At ip {ip} value is {state.Registers["X"]}");
                List<Instruction> next = new List<Instruction>();
                foreach (var ins in pending)
                {
                    if (!ins.TryExecute(state))
                    {
                        next.Add(ins);
                    }
                }

                pending = next;
            }
            ip++;
            Helpers.VerboseLine($"At ip {ip} value is {state.Registers["X"]}");
        }
        private async Task RunSequential(IAsyncEnumerable<string> data)
        {
            int ip = 1;
            List<int> checkPoints = new List<int> { 20, 60, 100, 140, 180, 220 };
            int strength = 0;
            ExecutionState state = new();
            await foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line))
                    break;

                var ins = ParseInstruction(line);
                do
                {
                    if (checkPoints.Count > 0 && ip == checkPoints[0])
                    {
                        var part = ip * state.Registers["X"];
                        strength += part;
                        checkPoints.RemoveAt(0); ;
                        Console.WriteLine($"Checkpoint at {ip} = +{part} {strength}");
                    }
                    ip++;
                } while (!ins.TryExecute(state));
                Helpers.VerboseLine($"At ip {ip} value is {state.Registers["X"]}");
            }
        }
        private async Task RunRender(IAsyncEnumerable<string> data)
        {
            var e = data.GetAsyncEnumerator();
            Instruction ins = null;
            var state = new ExecutionState();
            for (int i = 1; i <= 240; i++)
            {
                if (ins == null)
                {
                    await e.MoveNextAsync();
                    ins = ParseInstruction(e.Current);
                }
                
                if (Math.Abs(state.Registers["X"] - ((i-1) % 40)) <= 1)
                {
                    Console.Write("#");
                }
                else
                {
                    Console.Write(".");
                }
                
                if (ins.TryExecute(state))
                {
                    ins = null;
                }


                if (i % 40 == 0)
                {
                    Console.WriteLine();
                }
            }
        }

        private static Instruction ParseInstruction(string line)
        {
            var parts = line.Split(' ');
            switch (parts[0])
            {
                case "noop":
                    return new DelayInstruction();
                case "addx":
                    return new AddXInstruction(int.Parse(parts[1]));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("parts[0]");
            }
        }
    }
}