using System;
using System.Collections.Generic;

namespace SharpTiler
{
    internal static class Utility
    {
        internal static IEnumerable<T> Iterate<T>(T t, Func<T, T> func)
        {
            while (true)
            {
                yield return t;

                t = func(t);
            }
        }

        internal static IEnumerable<T> Repeatedly<T>(Func<T> func)
        {
            while (true)
            {
                yield return func();
            }
        }

        internal static uint HashSeed = 2166136261u;

        internal static uint HashCombine(uint hash, uint value)
        {
            return HashCombine(HashCombine(HashCombine(HashCombine(hash, (byte)(value >> 24)), (byte)(value >> 16)), (byte)(value >> 8)), (byte)value);
        }

        internal static uint HashCombine(uint hash, byte value)
        {
            hash ^= value;
            hash *= 16777619u;

            return hash;
        }
    }
}
