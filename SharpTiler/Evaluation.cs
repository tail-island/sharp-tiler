using static System.Runtime.Intrinsics.X86.Popcnt;

namespace SharpTiler
{
    public static class Evaluation
    {
        // 評価関数。
        public static int Evaluate(Shape blockShape, Shape obstacleShape)
        {
            var mergedLines = blockShape | obstacleShape;

            return 18 * blockShape.BitCount - 5 * Edge() - 105 * OneHole();

            int Edge()
            {
                var result = 0;

                // こんな単純なコードで評価できる理由は、スライドを参照してください。
                // あと、6とか4、3、2を掛け算しているのは、重さを統一するためです。例えば左上だと、周囲のマスは2つしかありません。これを2と表現すると、左上のマスが空いている場合のペナルティが他よりも軽くなってしまいます。

                {
                    var i =  0;  // 一番上の行

                    result += 6 * ((int)PopCount(mergedLines[i    ] << 1 & ~mergedLines[i] & 0x_8000_0000_0000_0000) +  // 一番左
                                   (int)PopCount(mergedLines[i + 1]      & ~mergedLines[i] & 0x_8000_0000_0000_0000));

                    result += 4 * ((int)PopCount(mergedLines[i    ] >> 1 & ~mergedLines[i] & 0x_0000_0000_0000_0001) +  // 一番右
                                   (int)PopCount(mergedLines[i + 1] >> 1 & ~mergedLines[i] & 0x_0000_0000_0000_0001) +
                                   (int)PopCount(mergedLines[i + 1]      & ~mergedLines[i] & 0x_0000_0000_0000_0001));

                    result += 3 * ((int)PopCount(mergedLines[i    ] >> 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +  // 残り
                                   (int)PopCount(mergedLines[i    ] << 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i + 1] >> 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i + 1]      & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe));
                }

                {
                    var i = 63;  // 一番下の行

                    result += 4 * ((int)PopCount(mergedLines[i - 1]      & ~mergedLines[i] & 0x_8000_0000_0000_0000) +
                                   (int)PopCount(mergedLines[i - 1] << 1 & ~mergedLines[i] & 0x_8000_0000_0000_0000) +
                                   (int)PopCount(mergedLines[i    ] << 1 & ~mergedLines[i] & 0x_8000_0000_0000_0000));

                    result += 6 * ((int)PopCount(mergedLines[i - 1]      & ~mergedLines[i] & 0x_0000_0000_0000_0001) +
                                   (int)PopCount(mergedLines[i    ] >> 1 & ~mergedLines[i] & 0x_0000_0000_0000_0001));

                    result += 3 * ((int)PopCount(mergedLines[i - 1]      & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i - 1] << 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i    ] >> 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i    ] << 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe));
                }

                for (var i = 1; i < 63; ++i)  // 残り
                {
                    result += 3 * ((int)PopCount(mergedLines[i - 1]      & ~mergedLines[i] & 0x_8000_0000_0000_0000) +
                                   (int)PopCount(mergedLines[i - 1] << 1 & ~mergedLines[i] & 0x_8000_0000_0000_0000) +
                                   (int)PopCount(mergedLines[i    ] << 1 & ~mergedLines[i] & 0x_8000_0000_0000_0000) +
                                   (int)PopCount(mergedLines[i + 1]      & ~mergedLines[i] & 0x_8000_0000_0000_0000));

                    result += 3 * ((int)PopCount(mergedLines[i - 1]      & ~mergedLines[i] & 0x_0000_0000_0000_0001) +
                                   (int)PopCount(mergedLines[i    ] >> 1 & ~mergedLines[i] & 0x_0000_0000_0000_0001) +
                                   (int)PopCount(mergedLines[i + 1] >> 1 & ~mergedLines[i] & 0x_0000_0000_0000_0001) +
                                   (int)PopCount(mergedLines[i + 1]      & ~mergedLines[i] & 0x_0000_0000_0000_0001));

                    result += 2 * ((int)PopCount(mergedLines[i - 1]      & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i - 1] << 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i    ] >> 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i    ] << 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i + 1] >> 1 & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe) +
                                   (int)PopCount(mergedLines[i + 1]      & ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe));
                }

                return result;
            }

            int OneHole()
            {
                var result = 0;

                {
                    var i =  0;

                    result += (int)PopCount(mergedLines[i    ] << 1 &
                                            mergedLines[i + 1]      &
                                            ~mergedLines[i] & 0x_8000_0000_0000_0000);

                    result += (int)PopCount(mergedLines[i    ] >> 1 &
                                            mergedLines[i + 1] >> 1 &
                                            mergedLines[i + 1]      &
                                            ~mergedLines[i] & 0x_0000_0000_0000_0001);

                    result += (int)PopCount(mergedLines[i    ] >> 1 &
                                            mergedLines[i    ] << 1 &
                                            mergedLines[i + 1] >> 1 &
                                            mergedLines[i + 1]      &
                                            ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe);
                }

                {
                    var i = 63;

                    result += (int)PopCount(mergedLines[i - 1]      &
                                            mergedLines[i - 1] << 1 &
                                            mergedLines[i    ] << 1 &
                                            ~mergedLines[i] & 0x_8000_0000_0000_0000);

                    result += (int)PopCount(mergedLines[i - 1]      &
                                            mergedLines[i    ] >> 1 &
                                            ~mergedLines[i] & 0x_0000_0000_0000_0001);

                    result += (int)PopCount(mergedLines[i - 1]      &
                                            mergedLines[i - 1] << 1 &
                                            mergedLines[i    ] >> 1 &
                                            mergedLines[i    ] << 1 &
                                            ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe);
                }

                for (var i = 1; i < 63; ++i)
                {
                    result += (int)PopCount(mergedLines[i - 1]      &
                                            mergedLines[i - 1] << 1 &
                                            mergedLines[i    ] << 1 &
                                            mergedLines[i + 1]      &
                                            ~mergedLines[i] & 0x_8000_0000_0000_0000);

                    result += (int)PopCount(mergedLines[i - 1]      &
                                            mergedLines[i    ] >> 1 &
                                            mergedLines[i + 1] >> 1 &
                                            mergedLines[i + 1]      &
                                            ~mergedLines[i] & 0x_0000_0000_0000_0001);

                    result += (int)PopCount(mergedLines[i - 1]      & 
                                            mergedLines[i - 1] << 1 &
                                            mergedLines[i    ] >> 1 &
                                            mergedLines[i    ] << 1 &
                                            mergedLines[i + 1] >> 1 &
                                            mergedLines[i + 1]      &
                                            ~mergedLines[i] & 0x_7fff_ffff_ffff_fffe);
                }

                return result;
            }
        }
    }
}
