using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp
{
    public readonly record struct Point2<T>(T X, T Y) : IConvertable<(T x, T y), Point2<T>>
        where T : IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>
    {
        public static implicit operator Point2<T>((T x, T y) p) => new(p.x, p.y);

        public Point2<T> Add(Point2<T> d)
        {
            return new Point2<T>(d.X + X, d.Y + Y);
        }

        public Point2<T> Add(T dx, T dy)
        {
            return new Point2<T>(dx + X, dy + Y);
        }

        public bool Equals(T x, T y)
        {
            return X == x && Y == y;
        }
    }

    public readonly record struct Point3<T>(T X, T Y, T Z) : IConvertable<(T x,T y, T z), Point3<T>>
        where T:IAdditionOperators<T,T,T>
    {
        public static implicit operator Point3<T>((T x, T y, T z) p) => new(p.x, p.y, p.z);

        public Point3<T> Add(Point3<T> d)
        {
            return new Point3<T>(d.X + X, d.Y + Y, d.Z + Z);
        }
        
        public Point3<T> Add(T dx, T dy, T dz)
        {
            return new Point3<T>(dx + X, dy + Y, dz + Z);
        }
    }

    public interface IConvertable<in T1, out T2> where T2 : IConvertable<T1, T2>
    {
        static abstract implicit operator T2(T1 p);
    }

    public readonly record struct Rect2<T>(T Left, T Top, T Right, T Bottom)
        where T : IAdditionOperators<T, T, T>, IComparisonOperators<T, T, bool>
    {
        public bool IsInBounds(Point2<T> p)
        {
            return p.X >= Left && p.X <= Right && p.Y >= Top && p.Y <= Bottom;
        }
    }
}