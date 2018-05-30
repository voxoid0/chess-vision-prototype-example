using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ChessVisionWin
{
    class OCVUtil
    {
        public static IEnumerable<Point> ToPoints(Point2d[] input)
        {
            return input.Select(p => new Point(Math.Round(p.X), Math.Round(p.Y)));
        }

        public static IEnumerable<Point> ToPoints(Point2f[] input)
        {
            return input.Select(p => new Point(Math.Round(p.X), Math.Round(p.Y)));
        }

        public static Point2d[] Point2fTo2d(Point2f[] input)
        {
            return input.Select(p => new Point2d(p.X, p.Y)).ToArray();
        }
    }
}
