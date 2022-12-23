using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp.solvers
{
    public class Problem07 : AsyncProblemBase
    {
        private class DirNode
        {
            public DirNode(string name, DirNode parent)
            {
                Name = name;
                Parent = parent;
            }

            public string Name { get; }
            public List<DirNode> Children { get; } = new();
            public List<FileNode> Files { get; } = new();
            public DirNode Parent { get; }

            public DirNode MoveDown(string path)
            {
                var child = Children.FirstOrDefault(d => d.Name == path);
                if (child == null)
                    Children.Add(child = new DirNode(path, this));
                return child;
            }

            public int GetSize()
            {
                return Files.Sum(f => f.Size) + Children.Sum(c => c.GetSize());
            }

            public string GetFullName()
            {
                if (Parent != null)
                    return Parent.GetFullName() + " :: " + Name;
                return Name;
            }
        }
        
        

        private record class FileNode(string Name, int Size);

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            DirNode root = new DirNode("/", null);
            DirNode current = root;
            List<DirNode> allNodes = new List<DirNode> { root };
            await foreach (var line in data)
            {
                if (line.StartsWith("$"))
                {
                    string[] parts = line.Split(' ');
                    switch (parts[1])
                    {
                        case "cd":
                            switch (parts[2])
                            {
                                case "..":
                                    current = current.Parent;
                                    break;
                                case "/":
                                    current = root;
                                    break;
                                default:
                                    allNodes.Add(current = current.MoveDown(parts[2]));
                                    break;
                            }
                            break;
                    }
                    continue;
                }

                string[] lsPart = line.Split(' ');
                switch (lsPart[0])
                {
                    case "dir":
                        break;
                    default:
                        current.Files.Add(new FileNode(lsPart[1], int.Parse(lsPart[0])));
                        break;
                }
            }

            foreach (var n in allNodes)
            {
                Helpers.VerboseLine($"  {n.GetFullName()} = {n.GetSize()}");
            }
            
            var big = allNodes.Where(n => n.GetSize() <= 100000);
            Helpers.VerboseLine("");
            
            Console.WriteLine($"Total sizes {big.Sum(b => b.GetSize())}");

            var free = 70000000 - root.GetSize();
            var needed = 30000000 - free;
            Helpers.VerboseLine($"Root is of size {root.GetSize()} leaving {free}, meaning we need {needed} more");
            var best = allNodes.OrderBy(n => n.GetSize()).First(n => n.GetSize() >= needed);
            Console.WriteLine($"Deleting {best.GetFullName()} will free up {best.GetSize()}");
        }
    }
}