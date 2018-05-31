using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ChessVisionWin
{
    class BoolSquareStatesRenderer
    {
        private const int SquareSize = 32;
        private readonly Scalar WhiteColor = Scalar.Gray;
        private readonly Scalar BlackColor = Scalar.DarkGray;
        public Mat Output { get; private set; }

        private Window outWin = new Window("Estimated Board State");

        public BoolSquareStatesRenderer()
        {
            Output = new Mat(new Size(SquareSize * 8, SquareSize * 8), MatType.CV_8UC3);
        }

        public void Render(bool[,] state)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var color = state[row, col] ? Scalar.White : 
                                (HistChessboardModel.IsWhiteSquare(row, col) ? WhiteColor : BlackColor);
                    Cv2.Rectangle(
                        img: Output, 
                        pt1: new Point(row * SquareSize, col * SquareSize),
                        pt2: new Point((row + 1) * SquareSize, (col + 1) * SquareSize), 
                        color: color, 
                        thickness: -1);
                }
            }
            outWin.ShowImage(Output);
        }
    }
}
