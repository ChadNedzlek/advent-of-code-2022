namespace ChadNedzlek.AdventOfCode.Y2022.CSharp
{
    public record struct Point2I(int X, int Y) : IConvertable<(int x,int y), Point2I>
    {
        public static implicit operator Point2I((int x, int y) p) => new Point2I(p.x, p.y);

        public Point2I Add(Point2I d)
        {
            return new Point2I(d.X + X, d.Y + Y);
        }
        
        public Point2I Add(int dx, int dy)
        {
            return new Point2I(dx + X, dy + Y);
        }
    }

    public record struct IPoint3(int X, int Y, int Z) : IConvertable<(int x,int y, int z), IPoint3>
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

    public interface IConvertable<T1, T2> where T2 : IConvertable<T1, T2>
    {
        static abstract implicit operator T2(T1 p);
    }

    public record struct Rect2I(int Left, int Top, int Right, int Bottom)
    {
        public bool IsInBounds(Point2I p)
        {
            return p.X >= Left && p.X <= Right && p.Y >= Top && p.Y <= Bottom;
        }
    }
}