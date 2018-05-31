using System;
using OpenCvSharp;

namespace ChessVisionWin
{
    class FgSegChessboardModel
    {
        private const double SquareMaskScale = 1.0; // 0.85;

        public Mat BoardToImageTransform { get; }
        public Mat ImageToBoardTransform { get; }
        public Mat BackgroundModel { get; }
        public Mat SquaresMask { get; set; }


        public FgSegChessboardModel(Mat boardToImageTransform, Mat imageToBoardTransform, Mat frame)
        {
            BoardToImageTransform = boardToImageTransform ?? throw new ArgumentNullException(nameof(boardToImageTransform));
            ImageToBoardTransform = imageToBoardTransform ?? throw new ArgumentNullException(nameof(imageToBoardTransform));

            BackgroundModel = frame.Clone(); // TODO: average multiple frames...
            SquaresMask = DrawSquaresMask(frame);
        }


        public static bool IsWhiteSquare(int row, int col)
        {
            return SquareColor(row, col) == 0;
        }

        public static int SquareColor(int row, int col)
        {
            return (row + col) % 2;
        }

        public Point2d[] GetSquarePoints(int row, int col, double scale = 1.0)
        {
            return GetSquarePoints(row, col, BoardToImageTransform, scale);
        }

        public static Point2d[] GetSquarePoints(int rowIn, int colIn, Mat boardToImageTransform, double scale = 1.0)
        {
            // TODO: make always correct for any board orientation in video
            int row = colIn;
            int col = 7 - rowIn;
            //int row = rowIn;
            //int col = colIn;

            // Board coordinates adjusted by scale
            double left = row + 0.5 - (0.5 * scale);
            double top = col + 0.5 - (0.5 * scale);
            double right = row + 0.5 + (0.5 * scale);
            double bottom = col + 0.5 + (0.5 * scale);

            return Cv2.PerspectiveTransform(
                new Point2d[]
                {
                    new Point2d(left, top),
                    new Point2d(right, top),
                    new Point2d(right, bottom),
                    new Point2d(left, bottom)
                },
                boardToImageTransform);
        }

        Mat DrawSquaresMask(Mat frame)
        {
            var result = new Mat(frame.Size(), MatType.CV_8U, Scalar.Black);
            for (int col = 0; col < 8; col++)
            {
                for (int row = 0; row < 8; row++)
                {
                    var squarePoints = OCVUtil.ToPoints(GetSquarePoints(row, col, SquareMaskScale));
                    Cv2.FillConvexPoly(result, squarePoints, Scalar.White);
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="dest">Destination matrix to hold the mask; must be the same size as the video frames used to initialize the model.</param>
        /// <param name="scale">Factor by which to scale the square from its center; 1.0 means no scaling; 0.5 means it will have half the width and height and a quarter of the area.</param>
        /// <returns></returns>
        public void DrawSquareMask(int row, int col, Mat dest, double scale = 1.0)
        {
            DrawSquareMask(row, col, dest, BoardToImageTransform, scale);
        }

        public static void DrawSquareMask(int row, int col, Mat dest, Mat boardToImageTransform, double scale = 1.0)
        {
            var points = OCVUtil.ToPoints(GetSquarePoints(row, col, boardToImageTransform, scale));
            Cv2.FillConvexPoly(dest, points, Scalar.White);
        }
    }
}