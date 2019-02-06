using System;
using System.Collections.Generic;
using System.Linq;

using static System.Linq.Enumerable;
using static System.Runtime.Intrinsics.X86.Popcnt;

namespace SharpTiler
{
    public sealed class Shape
    {
        public ulong[] Lines { get; }

        public int BitCount
        {
            get
            {
                var result = 0;

                for (var i = 0; i < Lines.Length; ++i)
                {
                    result += (int)PopCount(Lines[i]);
                }

                return result;

                // return Lines.Select(l => (int)PopCount(l)).Sum();
            }
        }

        public bool IsBlank
        {
            get
            {
                for (var i = 0; i < Lines.Length; ++i)
                {
                    if (Lines[i] != 0)
                    {
                        return false;
                    }
                }

                return true;

                // return Lines.All(l => l == 0);
            }
        }

        public Shape()
        {
            Lines = new ulong[64];
        }

        public Shape(ulong[] lines)
        {
            Lines = lines.ToArray();  // これが一番速いらしい。
        }

        public Shape(IEnumerable<ulong> lines)
            : this(lines.ToArray())
        {
            ;
        }

        public Shape(IEnumerable<Point> points)
            : this()
        {
            foreach (var p in points)
            {
                Lines[p.Y] |= 0x_8000_0000_0000_0000ul >> p.X;  // 分かりづらかったので、C++版とは左右を入れ替えました。 
            }
        }

        public static ulong[] operator|(Shape x, Shape y)  // 戻り値の型がちょっとアレだけど、勘弁してください。。。
        {
            var lines = new ulong[64];

            for (var i = 0; i < lines.Length; ++i)
            {
                lines[i] = x.Lines[i] | y.Lines[i];
            }

            return lines;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine , Lines.Select((l, y) => new string(' ', y) + string.Concat(Range(0, 64).Select(x => (0x_8000_0000_0000_0000ul >> x & l) != 0 ? '■' : '□'))));
        }
    }
}
