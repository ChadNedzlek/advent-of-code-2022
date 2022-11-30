// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace aoc
{
    public static class Helpers
    {
        public static void Deconstruct<T>(this T[] arr, out T a, out T b)
        {
            if (arr.Length != 2)
                throw new ArgumentException($"{nameof(arr)} must be 2 elements in length", nameof(arr));
            a = arr[0];
            b = arr[1];
        }

        public static void For<T>(this T[,] arr, Action<T[,], int, int, T> act)
        {
            for (int i0 = 0; i0 < arr.GetLength(0); i0++)
            for (int i1 = 0; i1 < arr.GetLength(1); i1++)
            {
                act(arr, i0, i1, arr[i0,i1]);
            }
        }

        public static void For<T>(this T[,] arr, Action<T[,], int, int> act)
        {
            For(arr, (arr, a, b, __) => act(arr, a, b));
        }

        public static void For<T>(this T[,] arr, Action<int, int> act)
        {
            For(arr, (_, a, b, __) => act(a, b));
        }
        
        public static IEnumerable<T> AsEnumerable<T>(this T value)
        {
            return Enumerable.Repeat(value, 1);
        }

        public static void AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            TValue add,
            Func<TValue, TValue> update)
        {
            if (dict.TryGetValue(key, out var existing))
            {
                dict[key] = update(existing);
            }
            else
            {
                dict.Add(key, add);
            }
        }
        
        public static void Increment<TKey>(
            this IDictionary<TKey, int> dict,
            TKey key,
            int amount = 1)
        {
            AddOrUpdate(dict, key, amount, i => i + amount);
        }
        
        public static void Increment<TKey>(
            this IDictionary<TKey, long> dict,
            TKey key,
            long amount = 1)
        {
            AddOrUpdate(dict, key, amount, i => i + amount);
        }
    }
}