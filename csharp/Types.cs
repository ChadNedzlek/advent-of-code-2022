using System;
using System.Collections;
using System.Collections.Generic;

namespace ChadNedzlek.AdventOfCode.Y2022.CSharp
{
    public record struct IPoint2(int X, int Y) : IConvertable<(int x,int y), IPoint2>
    {
        public static implicit operator IPoint2((int x, int y) p) => new IPoint2(p.x, p.y);
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

    public record struct Rect2I(int Left, int Top, int Right, int Bottom);

    public class Infinite2I<T> : IEnumerable<T>
    {
        private readonly Dictionary<(int, int), T> _sparse = new();
        private readonly Func<int, int, T> _populate = (_,_) => default;
        private int _min0, _min1, _max0, _max1;

        public Infinite2I()
        {
        }

        public Infinite2I(int length0, int length1)
        {
            _max0 = length0 - 1;
            _max1 = length1 - 1;
        }

        public Infinite2I(Func<int, int, T> populate) : this(0, 0, populate)
        {
        }

        public Infinite2I(int length0, int length1, Func<int, int, T> populate)
        {
            _max0 = length0 - 1;
            _max1 = length1 - 1;
            _populate = populate;
        }

        public T this[int i0, int i1]
        {
            get
            {
                if (_sparse.TryGetValue((i0, i1), out var value))
                    return value;
                return _populate(i0, i1);
            }
            set
            {

                _min0 = int.Min(_min0, i0);
                _min1 = int.Min(_min1, i1);
                
                _max0 = int.Max(_max0, i0);
                _max1 = int.Max(_max1, i1);
                
                _sparse[(i0, i1)] = value;
            }
        }

        public bool TrySet(int i0, int i1, T value)
        {
            if (i0 < _min0 || i0 > _max0 || i1 < _min1 || i1 > _max1)
            {
                return false;
            }

            _sparse[(i0, i1)] = value;
            return true;
        }

        public int GetLowerBound(int i) => i switch
        {
            0 => _min0,
            1 => _min1,
            _ => throw new ArgumentException(),
        };
        public int GetUpperBound(int i) =>  i switch
        {
            0 => _max0,
            1 => _max1,
            _ => throw new ArgumentException(),
        };
        public int GetLength(int i) => i switch
        {
            0 => _max0 - _min0 + 1,
            1 => _max1 - _min1 + 1,
            _ => throw new ArgumentException(),
        };

        public IEnumerator<T> GetEnumerator()
        {
            for (int i0 = _min0; i0 <= _max0; i0++)
            {
                for (int i1 = _min1; i1 <= _max1; i1++)
                {
                    if (_sparse.TryGetValue((i0, i1), out var value))
                    {
                        yield return value;
                    }

                    yield return _populate(i0, i1);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Clear()
        {
            _sparse.Clear();
        }
    }
}