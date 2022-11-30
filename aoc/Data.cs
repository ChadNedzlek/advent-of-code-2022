// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace aoc
{
    public class Data
    {
        public static async IAsyncEnumerable<string> GetData(int problem, string type = "real")
        {
            string root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            using var reader = new StreamReader(Path.Combine(root, "data", $"data-{problem:00}-{type}.txt"));
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
                yield return line;
        }

        public static async IAsyncEnumerable<ValueTuple<T1, T2, T3, T4>> As<T1, T2, T3, T4>(
            IAsyncEnumerable<string> data,
            [RegexPattern] string pattern)
        {
            await foreach (string line in data)
            {
                yield return Parse<T1, T2, T3, T4>(line, pattern);
            }
        }

        public static async IAsyncEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6>> As<T1, T2, T3, T4, T5, T6>(
            IAsyncEnumerable<string> data,
            [RegexPattern] string pattern)
        {
            await foreach (string line in data)
            {
                yield return Parse<T1, T2, T3, T4, T5, T6>(line, pattern);
            }
        }

        public static async IAsyncEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6, T7>> As<T1, T2, T3, T4, T5, T6, T7>(
            IAsyncEnumerable<string> data,
            [RegexPattern] string pattern)
        {
            await foreach (string line in data)
            {
                yield return Parse<T1, T2, T3, T4, T5, T6, T7>(line, pattern);
            }
        }


        public static async IAsyncEnumerable<T1> As<T1>(
            IAsyncEnumerable<string> data,
            [RegexPattern] string pattern)
        {
            await foreach (string line in data)
            {
                yield return Parse<T1>(line, pattern);
            }
        }

        public static (T1, T2, T3, T4) Parse<T1, T2, T3, T4>(string line, [RegexPattern] string pattern)
        {
            var m = Regex.Match(line, pattern);
            return (
                (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
                (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2)),
                (T3)Convert.ChangeType(m.Groups[3].Value, typeof(T3)),
                (T4)Convert.ChangeType(m.Groups[4].Value, typeof(T4))
            );
        }

        public static (T1, T2, T3, T4, T5, T6) Parse<T1, T2, T3, T4, T5, T6>(string line, [RegexPattern] string pattern)
        {
            var m = Regex.Match(line, pattern);
            return (
                (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
                (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2)),
                (T3)Convert.ChangeType(m.Groups[3].Value, typeof(T3)),
                (T4)Convert.ChangeType(m.Groups[4].Value, typeof(T4)),
                (T5)Convert.ChangeType(m.Groups[5].Value, typeof(T5)),
                (T6)Convert.ChangeType(m.Groups[6].Value, typeof(T6))
            );
        }

        public static (T1, T2, T3, T4, T5, T6, T7) Parse<T1, T2, T3, T4, T5, T6, T7>(string line, [RegexPattern] string pattern)
        {
            var m = Regex.Match(line, pattern);
            return (
                (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
                (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2)),
                (T3)Convert.ChangeType(m.Groups[3].Value, typeof(T3)),
                (T4)Convert.ChangeType(m.Groups[4].Value, typeof(T4)),
                (T5)Convert.ChangeType(m.Groups[5].Value, typeof(T5)),
                (T6)Convert.ChangeType(m.Groups[6].Value, typeof(T6)),
                (T7)Convert.ChangeType(m.Groups[7].Value, typeof(T7))
            );
        }

        public static (T1, T2, T3) Parse<T1, T2, T3>(string line, [RegexPattern] string pattern)
        {
            var m = Regex.Match(line, pattern);
            return (
                (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
                (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2)),
                (T3)Convert.ChangeType(m.Groups[3].Value, typeof(T3))
            );
        }

        public static (T1, T2) Parse<T1, T2>(string line, [RegexPattern] string pattern)
        {
            var m = Regex.Match(line, pattern);
            return (
                (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
                (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2))
            );
        }

        public static T1 Parse<T1>(string line, [RegexPattern] string pattern)
        {
            var m = Regex.Match(line, pattern);
            return (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1));
        }
    }
}