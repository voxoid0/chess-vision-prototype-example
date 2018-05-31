using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ChessVisionWin
{
    class HistogramPieceSegmenter
    {
        private readonly HistChessboardModel _histChessboardModel;
        private Mat difference;
        private Mat states;
        private Window diffWin;
        private SquareStatesRenderer squareStatesRenderer = new SquareStatesRenderer();

        public HistogramPieceSegmenter(HistChessboardModel histChessboardModel)
        {
            this._histChessboardModel = histChessboardModel;
            difference = new Mat(histChessboardModel.BackgroundModel.Size(), MatType.CV_16SC3); // histChessboardModel.BackgroundModel.Type());
            states = difference.Clone();
            diffWin = new Window("Difference from BG");
        }

        public void Do(IVideo video)
        {
            Mat frame = video.GetNextFrame();
            while (frame != null)
            {
                Cv2.Subtract(frame, _histChessboardModel.BackgroundModel, difference, _histChessboardModel.SquaresMask);
                diffWin.ShowImage(difference);

                var classif = CalcSquareClassifications(frame);
                squareStatesRenderer.Render(classif);

                frame = video.GetNextFrame();
            }
        }

        SquareClassificationScores[,] CalcSquareClassifications(Mat frame)
        {
            var result = new SquareClassificationScores[8,8];
            Mat squareMask = new Mat(frame.Size(), MatType.CV_8U);
            Mat squareHist = new Mat();
            var histSize = new[] { 16, 16, 16 };
            var histRanges = new[] { new Rangef(0f, 256f), new Rangef(0f, 256f), new Rangef(0f, 256f) };

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    squareMask.SetTo(Scalar.Black);
                    _histChessboardModel.DrawSquareMask(row, col, squareMask, 1.0);
                    Cv2.CalcHist(
                        images: new[] { frame },
                        channels: new int[] { 0, 1, 2 },
                        mask: squareMask,
                        hist: squareHist,
                        dims: 3,
                        histSize: histSize,
                        ranges: histRanges);

                    var method = HistCompMethods.Correl;
                    var likeWhitePiece = Cv2.CompareHist(
                        h1: squareHist,
                        h2: _histChessboardModel.XOnYHist[(int) ChessColor.White, HistChessboardModel.SquareColor(row, col)], 
                        method: method);
                    var likeBlackPiece = Cv2.CompareHist(
                        h1: squareHist,
                        h2: _histChessboardModel.XOnYHist[(int)ChessColor.Black, HistChessboardModel.SquareColor(row, col)],
                        method: method);
                    var likeEmpty = Cv2.CompareHist(
                        h1: squareHist,
                        h2: _histChessboardModel.XOnYHist[(int)SquareState.Empty, HistChessboardModel.SquareColor(row, col)],
                        method: method);
                    result[row, col] = new SquareClassificationScores(new [] { likeWhitePiece, likeBlackPiece, likeEmpty });
                }
            }
            return result;
        }

    }
}
