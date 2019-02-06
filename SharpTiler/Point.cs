using System;

namespace SharpTiler
{
    public struct Point : IComparable<Point>
    {
        public int Y { get; }
        public int X { get; }

        public Point(int y, int x)
        {
            Y = y;
            X = x;
        }

        public Point Flip()
        {
            return new Point(Y, 0 - X - Y);  // <https://www.redblobgames.com/grids/hexagonals/>を参照してください
        }

        public Point Rotate()
        {
            return new Point(X + Y, 0 - Y);  // <https://www.redblobgames.com/grids/hexagonals/>を参照してください
        }

        int IComparable<Point>.CompareTo(Point other)
        {
            if (Y < other.Y)
            {
                return -1;
            }

            if (Y > other.Y)
            {
                return  1;
            }

            if (X < other.X)
            {
                return -1;
            }

            if (X < other.X)
            {
                return  1;
            }

            return 0;
        }
    }
}
