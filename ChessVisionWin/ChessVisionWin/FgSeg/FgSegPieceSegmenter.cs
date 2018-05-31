using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ChessVisionWin
{
    class FgSegPieceSegmenter
    {
        private const double DiffAvgThreshold = 0.18 * 255;
        private readonly FgSegChessboardModel _histChessboardModel;
        private Mat difference;
        private Mat background;
        private Window diffWin;
        BoolSquareStatesRenderer stateRenderer = new BoolSquareStatesRenderer();

        public FgSegPieceSegmenter(FgSegChessboardModel histChessboardModel)
        {
            _histChessboardModel = histChessboardModel;
            difference = new Mat(histChessboardModel.BackgroundModel.Size(), MatType.CV_16SC3); // histChessboardModel.BackgroundModel.Type());
            background = new Mat();
            histChessboardModel.BackgroundModel.ConvertTo(background, MatType.CV_16SC3);
            diffWin = new Window("Difference from BG");
        }

        public void Do(IVideo video)
        {
            Mat frame = video.GetNextFrame();
            while (frame != null)
            {
                frame.ConvertTo(frame, MatType.CV_16SC3);
                Cv2.Subtract(frame, background, difference, _histChessboardModel.SquaresMask);
                difference = difference.Abs().ToMat();
                ShowDiff();

                bool[,] squareState = EstimateSquareStates(difference);
                stateRenderer.Render(squareState);

                frame = video.GetNextFrame();
            }
        }

        private bool[,] EstimateSquareStates(Mat diff)
        {
            var states = new bool[8, 8];
            Mat maskedDiff = new Mat(diff.Size(), diff.Type());
            Mat squareMask = new Mat(diff.Size(), MatType.CV_8U);
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    squareMask.SetTo(Scalar.Black);
                    _histChessboardModel.DrawSquareMask(row, col, squareMask, 1.0);
                    Scalar diffChanAvg = diff.Mean(squareMask);
                    double diffAvg = diffChanAvg.Val0 + diffChanAvg.Val1 + diffChanAvg.Val2; // + diffChanSum.Val3;
                    states[row, col] = diffAvg > DiffAvgThreshold;

                    //maskedDiff.SetTo(Scalar.Black);
                    //diff.CopyTo(maskedDiff, squareMask);
                    //Scalar diffChanSum = maskedDiff.Sum(); // TODO: only sum masked region, or bounding rectangle thereof
                }
            }

            return states;
        }

        private void ShowDiff()
        {
            Mat visibleDiff = new Mat();
            difference.ConvertTo(visibleDiff, MatType.CV_8UC3);
            diffWin.ShowImage(visibleDiff);
        }
    }
}
