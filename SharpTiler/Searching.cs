using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;

using static System.Linq.Enumerable;
using static SharpTiler.Evaluation;
using static SharpTiler.Rule;
using static SharpTiler.Utility;

namespace SharpTiler
{
    public static class Searching
    {
        // chokudaiサーチ。
        public static IEnumerable<Rule.Action> Search(Shape obstacleShape, IList<Polyhex> polyhexes, DateTime timeLimit)
        {
            // chokudaiサーチなので、複数の優先度付きキューを用意し、最初の優先度付きキューに初期状態を入れておきます。
            var queues = Repeatedly<IPriorityQueue<Node, int>>(() => new SimplePriorityQueue<Node, int>()).Take(polyhexes.Count + 1).ToArray(); queues.First().Enqueue(new Node(new Shape(), new Rule.Action[0]), int.MinValue);

            // 同じ局面を何度も探索するのは無意味なので、探索済み局面（のハッシュ）を格納するセットを用意します。
            var visited = new HashSet<int>();

            // すべての優先度付きキューが空になるか、制限時間になるまでループします。
            while (Range(0, queues.Length - 1).Any(i => queues[i].Count > 0) && DateTime.Now <= timeLimit)
            {
                // 最後までループ。
                for (var i = 0; i < queues.Length - 1 && DateTime.Now <= timeLimit; ++i)
                {
                    // whileの2回目以降の繰り返しの場合、優先度付きキューが空の場合があり得ます。その場合は、次の手へ。
                    if (queues[i].Count == 0)
                    {
                        continue;
                    }

                    // 優先度が最も高い（評価が最も良い）局面を取得します。
                    var node = queues[i].Dequeue();

                    // 当該局面での合法手のすべてでループします。
                    foreach (var action in GetActions(node.BlockShape, obstacleShape, polyhexes[i]).ToArray())  // ToArrayして遅延評価を断ち切ったら速くなったような気がするんだけど、勘違いかなぁ。。。
                    {
                        // 次の局面を取得します。
                        var nextBlockShape = GetNext(node.BlockShape, action);

                        // 探索済みの場合は無視（パスした場合はnextBlockShapeが変わらなくて、それだとパスした後の局面でストップしてしまうので、キャッシュの計算に何手目かも含めます）。
                        if (!visited.Add(GetHashCode(nextBlockShape, i)))
                        {
                            continue;
                        }

                        // これまでの手の集合に、今回の手を追加します（antionsという名前ですけど、これはanswerです。命名規約が壊れていてごめんなさい ）。
                        var nextActions = new Rule.Action[node.Actions.Length + 1]; node.Actions.CopyTo(nextActions, 0); nextActions[node.Actions.Length] = action;

                        // 次の手番のキューに追加します。1回目のループも2回目もn回目も同じ優先度付きキューに追加されますので、これまでに見つけた局面の中で最も良い局面から探索されます。chokudaiサーチ便利！
                        queues[i + 1].Enqueue(new Node(nextBlockShape, nextActions), -Evaluate(nextBlockShape, obstacleShape));
                    }
                }
            }

            // 最後までたどり着けなかった場合は、全部パスなので0点だけど合法な解答をしておきます。
            if (queues.Last().Count == 0)
            {
                return Repeat(new Rule.Action(null, 0, 0), polyhexes.Count);
            }

            // 見つかった解答の中から、スコアが高く、パスではない手が少ない（配置したポリへクスが少ない）ものを選びます。
            var best = queues.Last().Aggregate((acc, n) =>
            {
                if ((n.BlockShape.BitCount > acc.BlockShape.BitCount) || (n.BlockShape.BitCount == acc.BlockShape.BitCount && n.Actions.Where(a => a.TransformedPolyhex != null).Count() < acc.Actions.Where(a => a.TransformedPolyhex != null).Count()))
                {
                    return n;
                }

                return acc;
            });

            // デバッグ用にスコアと配置したポリへクスの数を出力します。
            Console.Error.WriteLine($"{best.BlockShape.BitCount}\t{best.Actions.Where(a => a.TransformedPolyhex != null).Count()}");

            return best.Actions;
        }

        private static int GetHashCode(Shape blockShape, int step)
        {
            var hash = HashSeed;

            for (var i = 0; i < blockShape.Lines.Length; ++i)
            {
                hash = HashCombine(HashCombine(hash, (uint)(blockShape.Lines[i] >> 32)), (uint)blockShape.Lines[i]);
            }

            hash = HashCombine(hash, (uint)step);

            return (int)hash;
        }

        private sealed class Node
        {
            public Shape         BlockShape { get; }
            public Rule.Action[] Actions    { get; }

            public Node(Shape blockShape, Rule.Action[] actions)
            {
                BlockShape = blockShape;
                Actions    = actions;
            }
        }
    }
}
