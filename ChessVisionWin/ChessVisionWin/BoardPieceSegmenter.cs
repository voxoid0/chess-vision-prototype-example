using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ChessVisionWin
{
    class BoardPieceSegmenter
    {
        private readonly ChessboardModel chessboardModel;
        private Mat difference;
        private Mat states;
        private Window diffWin;
        private SquareStatesRenderer squareStatesRenderer = new SquareStatesRenderer();

        public BoardPieceSegmenter(ChessboardModel chessboardModel)
        {
            this.chessboardModel = chessboardModel;
            difference = new Mat(chessboardModel.BackgroundModel.Size(), chessboardModel.BackgroundModel.Type());
            states = difference.Clone();
            diffWin = new Window("Difference from BG");
        }

        public void Do(IVideo video)
        {
            Mat frame = video.GetNextFrame();
            while (frame != null)
            {
                Cv2.Subtract(chessboardModel.BackgroundModel, frame, difference, chessboardModel.SquaresMask);
                diffWin.ShowImage(difference);

                var classif = CalcSquareClassifications(difference);
                squareStatesRenderer.Render(classif);

                frame = video.GetNextFrame();
            }
        }

        SquareClassificationScores[,] CalcSquareClassifications(Mat difference)
        {
            var result = new SquareClassificationScores[8,8];
            Mat squareMask = new Mat(difference.Size(), MatType.CV_8U);
            Mat squareHist = new Mat();
            var histSize = new[] { 16, 16, 16 };
            var histRanges = new[] { new Rangef(0f, 256f), new Rangef(0f, 256f), new Rangef(0f, 256f) };

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    chessboardModel.GetSquareMask(row, col, squareMask, 1.0);
                    Cv2.CalcHist(
                        images: new[] { difference },
                        channels: new int[] { 0, 1, 2 },
                        mask: squareMask,
                        hist: squareHist,
                        dims: 3,
                        histSize: histSize,
                        ranges: histRanges);

                    var likeWhitePiece = Cv2.CompareHist(
                        h1: squareHist,
                        h2: chessboardModel.XOnYHist[(int) ChessColor.White, ChessboardModel.SquareColor(row, col)], 
                        method: HistCompMethods.Correl);
                    var likeBlackPiece = Cv2.CompareHist(
                        h1: squareHist,
                        h2: chessboardModel.XOnYHist[(int)ChessColor.Black, ChessboardModel.SquareColor(row, col)],
                        method: HistCompMethods.Correl);
                    var likeEmpty = 0.8;
                    result[row, col] = new SquareClassificationScores(new [] { likeWhitePiece, likeBlackPiece, likeEmpty });
                }
            }
            return result;
        }

    }

    enum SquareState
    {
        White, Black, Empty
    }

    class SquareClassificationScores
    {
        private readonly double[] scores;

        public SquareClassificationScores(double[] scores)
        {
            this.scores = scores;
        }

        public SquareState MostLikely
        {
            get
            {
                if (White >= Black && White > Empty)
                    return SquareState.White;
                else if (Black >= White && Black > Empty)
                    return SquareState.Black;
                else
                    return SquareState.Empty;
            }
        }
            
        public double Empty => scores[(int) SquareState.Empty];
        public double White => scores[(int)SquareState.White];
        public double Black => scores[(int)SquareState.Black];
    }
}
