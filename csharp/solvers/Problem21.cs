using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem21 : SyncProblemBase
    {
        public abstract class Monkey
        {
            public abstract long? GetResult(Dictionary<string, Monkey> monkeys);
            public abstract void PushResult(Dictionary<string, Monkey> monkeys, long result);
        }

        public class WaitMonkey : Monkey
        {
            public readonly string AMonkey;
            public readonly string BMonkey;
            public readonly char Operation;

            public WaitMonkey(string aMonkey, string bMonkey, char operation)
            {
                AMonkey = aMonkey;
                BMonkey = bMonkey;
                Operation = operation;
            }

            public override void PushResult(Dictionary<string, Monkey> monkeys, long result)
            {
                var a = monkeys[AMonkey];
                var aResult = a.GetResult(monkeys);
                var b = monkeys[BMonkey];
                var bResult = b.GetResult(monkeys);
                if (aResult.HasValue && bResult.HasValue)
                    return;

                if (aResult.HasValue)
                {
                    switch (Operation)
                    {
                        case '+':
                            b.PushResult(monkeys, result - aResult.Value);
                            return;
                        case '-':
                            b.PushResult(monkeys, aResult.Value - result);
                            return;
                        case '*':
                            b.PushResult(monkeys, result / aResult.Value);
                            return;
                        case '/':
                            b.PushResult(monkeys, aResult.Value / result);
                            return;
                        case '=': 
                            b.PushResult(monkeys, aResult.Value);
                            return;
                        default:
                            throw new NotSupportedException();
                  
                    }
                }

                if (!bResult.HasValue)
                    throw new ArgumentException();
                
                switch (Operation)
                {
                    case '+':
                        a.PushResult(monkeys, result - bResult.Value);
                        return;
                    case '-':
                        a.PushResult(monkeys, result + bResult.Value);
                        return;
                    case '*':
                        a.PushResult(monkeys, result / bResult.Value);
                        return;
                    case '/':
                        a.PushResult(monkeys, result * bResult.Value);
                        return;
                    case '=': 
                        a.PushResult(monkeys, bResult.Value);
                        return;
                    default:
                        throw new NotSupportedException();
                }
            }

            public override long? GetResult(Dictionary<string, Monkey> monkeys)
            {
                var a = monkeys[AMonkey].GetResult(monkeys);
                var b = monkeys[BMonkey].GetResult(monkeys);
                switch (Operation)
                {
                    case '+': return a + b;
                    case '-': return a - b;
                    case '*': return a * b;
                    case '/': return a / b;
                    case '=': return a == b ? 1 : 0;
                    default: throw new NotSupportedException();
                }
            }
        }

        public class IntMonkey : Monkey
        {
            private long? _number;

            public IntMonkey(long? number)
            {
                _number = number;
            }

            public override long? GetResult(Dictionary<string, Monkey> monkeys)
            {
                return _number;
            }

            public override void PushResult(Dictionary<string, Monkey> monkeys, long result)
            {
                _number = result;
            }
        }

        protected override void ExecuteCore(IEnumerable<string> data)
        {
            Dictionary<string, Monkey> monkeys = new();
            foreach (var line in data)
            {
                var parts = line.Split(':');
                Monkey m;
                if (parts[1].Length == 12)
                {
                    m = new WaitMonkey(parts[1][1..5], parts[1][8..], parts[1][6]);
                }
                else
                {
                    m = new IntMonkey(long.Parse(parts[1]));
                }
                monkeys.Add(parts[0], m);
            }

            var rootMonkey = (WaitMonkey)monkeys["root"];
            var res = rootMonkey.GetResult(monkeys);
            Console.WriteLine($"Root says: {res}");

            var human = monkeys["humn"] = new IntMonkey(null);
            monkeys["root"] = rootMonkey = new WaitMonkey(rootMonkey.AMonkey, rootMonkey.BMonkey, '=');
            rootMonkey.PushResult(monkeys, 1);
            var humanRes = human.GetResult(monkeys);
            Console.WriteLine($"Human says {humanRes}");
        }
    }
}