using System;
using System.Collections.Generic;
using System.Linq;

using static SharpTiler.Utility;
using static System.Linq.Enumerable;

namespace SharpTiler
{
    public sealed class Polyhex
    {
        // 反転や回転済みのポリへクス。TransformedPolyhexesだとなんか長かったので変な名前になりました。。。
        public Transform[] Transforms { get; }

        public Polyhex(IEnumerable<Point> points)
        {
            // 反転や回転をした点の集合の集合を取得します。Iterate()は、Utilityで定義しています。
            var pointsCollection = Iterate(points, ps => ps.Select(p => p.Rotate())).Take(6).Concat(Iterate(points.Select(p => p.Flip()), ps => ps.Select(p => p.Rotate())).Take(6)).ToArray();

            // 反転や回転で同じ形になる場合は探索しても無駄なので枝狩りして、探索が必要な反転や回転だけを取得します。
            var operations = pointsCollection.Select((ps, o) => (NormalizeForCompare(ps), Operation: o)).Distinct(new OperatedPointsComparer()).Select(pso => pso.Operation);  // C# 7.0のタプル便利！

            // 反転や回転をした結果をTransformsに設定します。
            Transforms = operations.Select(o =>
            {
                // (+32, +32)して、はみだしを防ぎます。
                var polyhexShapePoints = pointsCollection[o].Select(p => new Point(p.Y + 32, p.X + 32));

                // ポリへクスのシェイプを作成します。
                var polyhexShape      = new Shape(polyhexShapePoints);

                // 周囲のブロックのシェイプを作成します。
                var surroundingsShape = new Shape(Range(0, 64).Select(i =>
                {
                    // 評価関数と同じテクニックで、周囲のブロックのシェイプを作成します。
                    switch (i)
                    {
                        case  0: return (polyhexShape.Lines[i    ] >> 1 |
                                         polyhexShape.Lines[i    ] << 1 |
                                         polyhexShape.Lines[i + 1] >> 1 |
                                         polyhexShape.Lines[i + 1]     ) &
                                        ~polyhexShape.Lines[i];

                        case 63: return (polyhexShape.Lines[i - 1]      |
                                         polyhexShape.Lines[i - 1] << 1 |
                                         polyhexShape.Lines[i    ] >> 1 |
                                         polyhexShape.Lines[i    ] << 1) &
                                        ~polyhexShape.Lines[i];

                        default: return (polyhexShape.Lines[i - 1]      |
                                         polyhexShape.Lines[i - 1] << 1 |
                                         polyhexShape.Lines[i    ] >> 1 |
                                         polyhexShape.Lines[i    ] << 1 |
                                         polyhexShape.Lines[i + 1] >> 1 |
                                         polyhexShape.Lines[i + 1]     ) &
                                        ~polyhexShape.Lines[i];
                    }
                }));

                // Rule.GetNextActions()で必要となる、上下左右の位置もここで設定しておきます。
                return new Transform(o, polyhexShapePoints.Select(p => p.X).Min(), polyhexShapePoints.Select(p => p.Y).Min(), polyhexShapePoints.Select(p => p.X).Max(), polyhexShapePoints.Select(p => p.Y).Max(), polyhexShape, surroundingsShape);
            }).ToArray();
        }

        public sealed class Transform
        {
            public int   Operation         { get; }
            public int   Left              { get; }
            public int   Top               { get; }
            public int   Right             { get; }
            public int   Bottom            { get; }
            public Shape PolyhexShape      { get; }
            public Shape SurroundingsShape { get; }

            public Transform(int operation, int left, int top, int right, int bottom, Shape polyhexShape, Shape surroundingsShape)
            {
                Operation         = operation;
                Left              = left;
                Top               = top;
                Right             = right;
                Bottom            = bottom;
                PolyhexShape      = polyhexShape;
                SurroundingsShape = surroundingsShape;
            }
        }

        private IEnumerable<Point> NormalizeForCompare(IEnumerable<Point> points)
        {
            var minY = points.Select(p => p.Y).Min();
            var minX = points.Select(p => p.X).Min();

            return points.Select(p => new Point(minY - p.Y, minX - p.X)).OrderBy(p => p);
        }

        private struct OperatedPointsComparer : IEqualityComparer<(IEnumerable<Point> Points, int Operation)>
        {
            public bool Equals((IEnumerable<Point> Points, int Operation) x, (IEnumerable<Point> Points, int Operation) y)
            {
                return x.Points.SequenceEqual(y.Points);
            }

            public int GetHashCode((IEnumerable<Point> Points, int Operation) obj)
            {
                return (int)obj.Points.Aggregate(HashSeed, (acc, p) => HashCombine(HashCombine(acc, (uint)p.Y), (uint)p.X));
            }
        }
    }
}
