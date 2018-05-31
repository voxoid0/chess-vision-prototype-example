using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace ChessVisionWin
{
    class HistPlotter
    {
        private readonly int width = 64;
        private readonly int height = 128;
        private readonly Scalar[] chanColor = new[] {Scalar.Blue, Scalar.Green, Scalar.Red};

        public Mat Plot(Mat hist, string name)
        {
            Mat plot = new Mat(new Size(width, height), MatType.CV_8UC3, Scalar.Black);
            var win = new Window(name);
            win.ShowImage(plot);
            return plot;
        }
    }
}
