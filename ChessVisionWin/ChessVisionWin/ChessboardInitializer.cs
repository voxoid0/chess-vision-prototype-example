using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ChessVisionWin
{
    /// <summary>
    /// Abstract video input, to allow for pre-formatting of each frame, such as
    /// resizing, converting color space, HDR, etc.
    /// </summary>
    public interface IVideo
    {
        Mat GetNextFrame();
    }

    class ChessboardInitializer
    {
        // TODO: Optional<Mat>
        public ChessboardModel Do(IVideo video)
        {
            ChessboardModel model = null;
            Mat frame;
            Mat gray = new Mat();
            Mat thresh = new Mat();
            var corners = new Point2f[4];
            var patternSize = new Size(7, 3);

            do
            {
                frame = video.GetNextFrame();
                if (frame != null && frame.Width + frame.Height > 0)
                {
                    Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                    //Cv2.MedianBlur(gray, gray, 5); // Disallows chessboard to be found, because of how it opens/closes corners
                    Cv2.AdaptiveThreshold(gray, thresh,
                        maxValue: 255.0,
                        adaptiveMethod: AdaptiveThresholdTypes.GaussianC,
                        thresholdType: ThresholdTypes.Binary,
                        blockSize: (gray.Height / 2) | 1,
                        c: 0.0);
                    //threshWin.ShowImage(thresh);

                    var found = Cv2.FindChessboardCorners(thresh, patternSize, out corners,
                        ChessboardFlags.None); //, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);

                    //frame.CopyTo(output);
                    //Cv2.DrawChessboardCorners(output, patternSize, corners, found);
                    //if (!found) Console.Out.WriteLine("Chessboard not found :( ");

                    if (found)
                    {
                        var boardPoints = new Point2d[21];
                        Point2d[] foundPoints = OCVUtil.Point2fTo2d(corners);
                        for (int c = 0; c < 7; c++)
                        {
                            for (int r = 0; r < 3; r++)
                            {
                                boardPoints[r * 7 + c] = new Point2d((c + 1.0), (r + 3.0));
                            }
                        }

                        var boardToImageTransform = Cv2.FindHomography(boardPoints, foundPoints);
                        var imageToBoardTransform =
                            boardToImageTransform.Inv(); //Cv2.FindHomography(foundPoints, boardPoints);
                        model = new ChessboardModel(boardToImageTransform, imageToBoardTransform, frame);
                    }
                }
            } while (frame != null && model == null);

            return model;
        }
    }

    enum ChessColor
    {
        White,
        Black
    }

    class ChessboardModel
    {
        private const double SquareMaskScale = 1.0; // 0.85;

        public Mat BoardToImageTransform { get; }
        public Mat ImageToBoardTransform { get; }
        public Scalar WhiteSquareColor { get; }
        public Scalar BlackSquareColor { get; }
        public Mat BackgroundModel { get; }
        public Mat SquaresMask { get; set; }
        public Mat[,] XOnYHist { get; }
        public Mat InitFrame { get; }

        public ChessboardModel(Mat boardToImageTransform, Mat imageToBoardTransform, Mat frame)
        {
            this.BoardToImageTransform = boardToImageTransform ?? throw new ArgumentNullException(nameof(boardToImageTransform));
            this.ImageToBoardTransform = imageToBoardTransform ?? throw new ArgumentNullException(nameof(imageToBoardTransform));

            Scalar evenColor = GetMeanColor(0, frame);
            Scalar oddColor = GetMeanColor(1, frame);
            // TODO: proper color conversion
            if (evenColor.Val0 + evenColor.Val1 + evenColor.Val2 > oddColor.Val0 + oddColor.Val1 + oddColor.Val2)
            {
                WhiteSquareColor = evenColor;
                BlackSquareColor = oddColor;
            }
            BackgroundModel = DrawBackgroundModel(frame);
            SquaresMask = DrawSquaresMask(frame);
            XOnYHist = CreateXOnYHistograms(frame);
            InitFrame = frame.Clone();
        }

        /// <summary>
        /// Creates a histogram for each of the 4 combinations of piece color on square color, differenced with the background model.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        private Mat[,] CreateXOnYHistograms(Mat frame)
        {
            using (var xOnYWin = new Window("X On Y"))
            {
                Mat[,] xOnYHist = new Mat[2, 2] { { new Mat(), new Mat() }, { new Mat(), new Mat() } };
                Mat difference = new Mat(frame.Size(), frame.Type());
                for (int pieceColor = 0; pieceColor < 2; pieceColor++)
                {
                    for (int squareColor = 0; squareColor < 2; squareColor++)
                    {
                        Cv2.Subtract(BackgroundModel, frame, difference, SquaresMask);
                        Mat xOnYMask = CreateXOnYMaskForInitialPositions(frame, pieceColor, squareColor);

                        xOnYWin.ShowImage(xOnYMask);
                        Cv2.WaitKey();

                        Cv2.CalcHist(
                            images: new[] { difference },
                            channels: new int[] { 0, 1, 2 },
                            mask: xOnYMask,
                            hist: xOnYHist[pieceColor, squareColor],
                            dims: 3,
                            histSize: new[] { 16, 16, 16 },
                            ranges: new[] { new Rangef(0f, 256f), new Rangef(0f, 256f), new Rangef(0f, 256f) });
                    }
                }
                return xOnYHist;
            }
        }

        /// <summary>
        /// Creates a mask covering all 8 squares containing the given combination of piece color and square color, 
        /// assuming the chess pieces are in their initial positions.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="pieceColor"></param>
        /// <param name="squareColor"></param>
        /// <returns></returns>
        Mat CreateXOnYMaskForInitialPositions(Mat frame, int pieceColor, int squareColor)
        {
            Mat mask = new Mat(frame.Size(), MatType.CV_8U, Scalar.Black);
            int rowOffset = 6 * pieceColor; // Black (1) will be offset 6 squares, at rows 6 and 7
            for (int row = rowOffset; row < 2 + rowOffset; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if ((row + col + squareColor) % 2 == 0)
                    {
                        GetSquareMask(row, col, mask, SquareMaskScale);
                    }
                }
            }

            return mask;
        }

        Scalar GetMeanColor(int evenOrOddSquares, Mat frame)
        {
            Mat mask = new Mat(frame.Size(), MatType.CV_8U);
            for (int col = 0; col < 8; col++)
            {
                for (int row = 2; row < 6; row++)
                {
                    if ((col + row + evenOrOddSquares) % 2 == 0)
                    {
                        GetSquareMask(row, col, mask);
                    }
                }
            }

            Scalar mean = Cv2.Mean(frame, mask);
            return mean;
        }

        Mat DrawBackgroundModel(Mat frame)
        {
            var result = new Mat(frame.Size(), frame.Type());
            for (int col = 0; col < 8; col++)
            {
                for (int row = 0; row < 8; row++)
                {
                    var color = IsWhiteSquare(row, col) ? WhiteSquareColor : BlackSquareColor;
                    var squarePoints = OCVUtil.ToPoints(GetSquarePoints(row, col));
                    Cv2.FillConvexPoly(result, squarePoints, color);
                }
            }
            return result;
        }

        public static bool IsWhiteSquare(int row, int col)
        {
            return ((row + col) % 2 == 0);
        }

        public static int SquareColor(int row, int col)
        {
            return (row + col) % 2;
        }

        public Point2d[] GetSquarePoints(int row, int col, double scale = 1.0)
        {
            return GetSquarePoints(row, col, BoardToImageTransform, scale);
        }

        public static Point2d[] GetSquarePoints(int row, int col, Mat boardToImageTransform, double scale = 1.0)
        {
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

        public Point2d[] GetBoardCorners()
        {
            return Cv2.PerspectiveTransform(
                new Point2d[]
                {
                    new Point2d(0.0, 0.0),
                    new Point2d(0.0, 8.0),
                    new Point2d(8.0, 8.0),
                    new Point2d(8.0, 0.0)
                },
                BoardToImageTransform);
        }

        Mat DrawBoardMask(Mat frame)
        {
            var result = new Mat(frame.Size(), MatType.CV_8U);
            var corners = OCVUtil.ToPoints(GetBoardCorners());
            Cv2.FillConvexPoly(result, corners, Scalar.White);
            return result;
        }

        Mat DrawSquaresMask(Mat frame)
        {
            var result = new Mat(frame.Size(), MatType.CV_8U);
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
        public void GetSquareMask(int row, int col, Mat dest, double scale = 1.0)
        {
            GetSquareMask(row, col, dest, BoardToImageTransform, scale);
        }

        public static void GetSquareMask(int row, int col, Mat dest, Mat boardToImageTransform, double scale = 1.0)
        {
            var points = OCVUtil.ToPoints(GetSquarePoints(row, col, boardToImageTransform, scale));
            Cv2.FillConvexPoly(dest, points, Scalar.White);
        }
    }
}
