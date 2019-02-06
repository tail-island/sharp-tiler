
using System;
using System.Collections.Generic;
using System.Linq;

using static System.Linq.Enumerable;
using static SharpTiler.Searching;

namespace SharpTiler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var obstacleShape = ReadObstacleShape();
            var polyhexes     = ReadPolyhexes().ToArray();

            WriteActions(Search(obstacleShape, polyhexes, DateTime.Now + new TimeSpan(9 * 1000 * 1000 * 10)));
        }

        private static Shape ReadObstacleShape()
        {
            var width  = int.Parse(Console.ReadLine());
            var height = int.Parse(Console.ReadLine());

            var shape = new Shape(new Func<IEnumerable<Point>>(() => Range(0, int.Parse(Console.ReadLine())).Select(i => { var xy = Console.ReadLine().Split(",").Select(int.Parse).ToArray(); return new Point(xy[1], xy[0]); }))());

            for (var y = 0; y < shape.Lines.Length; ++y)
            {
                shape.Lines[y] |= (y < height ? Range(width, 64 - width) : Range(0, 64)).Aggregate(0ul, (acc, x) => acc |= 0x_8000_0000_0000_0000ul >> x);
            }

            // 区切りの空行を読み込みます。
            Console.ReadLine();

            return shape;
        }

        private static IEnumerable<Polyhex> ReadPolyhexes()
        {
            return Range(0, int.Parse(Console.ReadLine())).Select(i => new Polyhex(new Func<IEnumerable<Point>>(() => Console.ReadLine().Split(";").Skip(1).Select(s => { var xy = s.Split(",").Select(int.Parse).ToArray(); return new Point(xy[1], xy[0]); }))()));
        }

        private static void WriteActions(IEnumerable<Rule.Action> actions)
        {
            foreach (var action in actions)
            {
                if (action.TransformedPolyhex == null)
                {
                    Console.WriteLine("P");
                    continue;
                }

                Console.WriteLine($"{(32 - action.X) & 0x3f},{(32 - action.Y) & 0x3f};{action.TransformedPolyhex.Operation / 6};{(action.TransformedPolyhex.Operation % 6) * 60}");
            }
        }
    }
}
