using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem13 : AsyncProblemBase
    {
        public abstract record Packet() : IComparable<Packet>
        {
            public abstract int CompareTo(Packet other);
        }

        public record ListPacket(params Packet[] Packets) : Packet
        {
            public override int CompareTo(Packet other)
            {
                switch (other)
                {
                    case IntPacket i:
                        return CompareTo(new ListPacket(i));
                    case ListPacket l:
                        for (int i = 0;; i++)
                        {
                            if (i == Packets.Length)
                            {
                                if (i == l.Packets.Length)
                                {
                                    return 0;
                                }

                                return -1;
                            }

                            if (i == l.Packets.Length)
                                return 1;

                            var sub = Packets[i].CompareTo(l.Packets[i]);
                            if (sub != 0)
                                return sub;
                        }
                    default:
                        throw new NotSupportedException();
                }
            }

            public override string ToString()
            {
                return "{" + string.Join("-", Packets.Select(p => p.ToString())) + "}";
            }
        }

        public record IntPacket(int Value) : Packet
        {
            public override int CompareTo(Packet other)
            {
                switch (other)
                {
                    case IntPacket i:
                        return Value.CompareTo(i.Value);
                    case ListPacket l:
                        return new ListPacket(this).CompareTo(l);
                    default:
                        throw new NotSupportedException();
                }
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            List<(ListPacket a, ListPacket b)> pairs = new();
            var enumerator = data.GetAsyncEnumerator();
            int i = 0;
            var inOrder = new List<int>();
            var allPackets = new List<ListPacket>();
            while (await enumerator.MoveNextAsync())
            {
                i++;
                var a = (ListPacket)ParseLine(enumerator.Current, out _);
                await enumerator.MoveNextAsync();
                var b = (ListPacket)ParseLine(enumerator.Current, out _);
                await enumerator.MoveNextAsync();
                if (a.CompareTo(b) < 0)
                {
                    inOrder.Add(i);
                }

                allPackets.Add(a);
                allPackets.Add(b);
            }
            Console.WriteLine($"The pairs in order sum to {inOrder.Sum()} ({string.Join(",", inOrder)})");
            var div2 = new ListPacket(new ListPacket(new IntPacket(2)));
            var div6 = new ListPacket(new ListPacket(new IntPacket(6)));
            allPackets.Add(div2);
            allPackets.Add(div6);
            allPackets.Sort();
            int iDiv2 = allPackets.IndexOf(div2) + 1;
            int iDiv6 = allPackets.IndexOf(div6) + 1;
            Console.Write($"Dividers at {iDiv2} and {iDiv6}, decoder key is {iDiv2 * iDiv6}");
        }

        private Packet ParseLine(ReadOnlySpan<char> line, out ReadOnlySpan<char> rest)
        {
            if (line[0] == '[')
            {
                if (line[1] == ']')
                {
                    rest = line[2..];
                    return new ListPacket();                    
                }

                List<Packet> subPackets = new List<Packet>();
                while (line[0] != ']')
                {
                    subPackets.Add(ParseLine(line[1..], out line));
                }

                rest = line[1..];
                return new ListPacket(subPackets.ToArray());
            }
            int i=0;
            while (i < line.Length && char.IsDigit(line[i]))
            {
                i++;
            }

            var value = int.Parse(line[..i]);
            rest = line[i..];
            return new IntPacket(value);
        }
    }
}