using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ChadNedzlek.AdventOfCode.DataModule;
using JetBrains.Annotations;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp
{
    public record struct IPoint2(int X, int Y)
    {
        public static implicit operator IPoint2((int x, int y) p) => new IPoint2(p.x, p.y);
    }

    public record struct IPoint3(int X, int Y, int Z)
    {
        public static implicit operator IPoint3((int x, int y, int z) p) => new IPoint3(p.x, p.y, p.z);
    }

    public record struct LPoint2(long X, long Y)
    {
        public static implicit operator LPoint2((long x, long y) p) => new LPoint2(p.x, p.y);
    }

    public record struct LPoint3(long X, long Y, long Z)
    {
        public static implicit operator LPoint3((long x, long y, long z) p) => new LPoint3(p.x, p.y, p.z);
    }
}