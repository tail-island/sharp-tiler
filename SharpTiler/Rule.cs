using System.Collections.Generic;

namespace SharpTiler
{
    public static class Rule
    {
        // 合法手の一覧を取得します。
        public static IEnumerable<Action> GetActions(Shape blockShape, Shape obstacleShape, Polyhex polyhex)
        {
            // まずは、パスする手を返します。
            yield return new Action(null, 0, 0);  // パスの分。

            // 重なり判定では配置済みブロックと障害ブロックの区別はありませんので、事前にマージしておきます。
            var mergedLines = blockShape | obstacleShape;

            foreach (var transformedPolyhex in polyhex.Transforms)
            {
                // yは、ポリへクスを「上」にどれだけ移動させるかを表現します。募集要項と逆でごめんなさい。
                for (var y = 0; y < 64; ++y)
                {
                    // yをポリへクスを「上」にどれだけ移動させるかにしたので、「ポリへクスの上下の間にyがある場合はポリへクスが分割されるので無視」が可能になります。
                    if (transformedPolyhex.Top < y && y <= transformedPolyhex.Bottom)
                    {
                        continue;
                    }

                    // xは、ポリへクスを「左」にどれだけ移動させるかを表現します。募集要項と逆でごめんなさい。
                    for (var x = 0; x < 64; ++x)
                    {
                        // xをポリへクスを「右」にどれだけ移動させるかにしたので、「ポリへクスの左右の間にxがある場合はポリへクスが分割されるので無視」が可能になります。
                        if (transformedPolyhex.Left < x && x <= transformedPolyhex.Right)
                        {
                            continue;
                        }

                        // 重なっていたり、接続していなかったりする場合は無視。IsOverlappedやIsConnectedは、C# 7.0のローカル関数で実装しています。
                        if (IsOverlapped() || !IsConnected())
                        {
                            continue;
                        }

                        // アクションを返します。
                        yield return new Action(transformedPolyhex, y, x);

                        // 配置済みブロックや障害物と重なっているかを返します。
                        bool IsOverlapped()
                        {
                            var i = transformedPolyhex.Top;
                            var j = i - y;

                            while (i <= transformedPolyhex.Bottom)
                            {
                                // ANDした結果が0でなければ、重なっています。
                                if (((transformedPolyhex.PolyhexShape.Lines[i] << x | transformedPolyhex.PolyhexShape.Lines[i] >> (64 - x)) & mergedLines[j & 0x3f]) != 0)
                                {
                                    return true;
                                }

                                ++i;
                                ++j;
                            }

                            return false;
                        }

                        // 配置済みブロックとつながっているかを返します。
                        bool IsConnected()
                        {
                            // 初回はつながっていなくてもOK。
                            if (blockShape.IsBlank)
                            {
                                return true;
                            }

                            // ポリへクスの上下の位置に合わせて、調査する範囲の上下を設定します。
                            var top    = ((transformedPolyhex.Top    - y) & 0x3f) ==  0 ? transformedPolyhex.Top    : transformedPolyhex.Top    - 1;
                            var bottom = ((transformedPolyhex.Bottom - y) & 0x3f) == 63 ? transformedPolyhex.Bottom : transformedPolyhex.Bottom + 1;

                            // ポリへクスが一番左や一番右に配置されると、周囲のブロックは反対側の端にはみ出てしまいます。だから、必要に応じて一番右や一番左を無視するためのマスクを設定します。
                            var mask = (((transformedPolyhex.Left  - x) & 0x3f) ==  0 ? 0x_ffff_ffff_ffff_fffe : 0x_ffff_ffff_ffff_ffff) &
                                       (((transformedPolyhex.Right - x) & 0x3f) == 63 ? 0x_7fff_ffff_ffff_ffff : 0x_ffff_ffff_ffff_ffff);

                            var i = top;
                            var j = i - y;

                            while (i <= bottom)
                            {
                                // ANDした結果が0でなければ、つながっています。
                                if (((transformedPolyhex.SurroundingsShape.Lines[i] << x | transformedPolyhex.SurroundingsShape.Lines[i] >> (64 - x)) & blockShape.Lines[j & 0x3f] & mask) != 0)
                                {
                                    return true;
                                }

                                ++i;
                                ++j;
                            }

                            return false;
                        }
                    }
                }
            }
        }

        // 次の局面を取得します。
        public static Shape GetNext(Shape blockShape, Action action)
        {
            // パスの場合は入力そのまま。
            if (action.TransformedPolyhex == null)
            {
                return blockShape;
            }

            var nextBlockShape = new Shape(blockShape.Lines);

            var i = action.TransformedPolyhex.Top;
            var j = i - action.Y;

            // 配置済みブロックにポリへクスをORで重ねます。
            while (i <= action.TransformedPolyhex.Bottom)
            {
                nextBlockShape.Lines[j & 0x3f] |= action.TransformedPolyhex.PolyhexShape.Lines[i] << action.X | action.TransformedPolyhex.PolyhexShape.Lines[i] >> (64 - action.X);

                ++i;
                ++j;
            }

            return nextBlockShape;
        }

        public sealed class Action
        {
            public Polyhex.Transform TransformedPolyhex { get; }  // パスの時はnull。
            public int               X                  { get; }
            public int               Y                  { get; }

            public Action(Polyhex.Transform transformedPolyhex, int y, int x)
            {
                TransformedPolyhex = transformedPolyhex;
                Y                  = y;
                X                  = x;
            }
        }
    }
}
