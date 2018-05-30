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
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                //Cv2.MedianBlur(gray, gray, 5); // Disallows chessboard to be found, because of how it opens/closes corners
                Cv2.AdaptiveThreshold(gray, thresh,
                    maxValue: 255.0,
                    adaptiveMethod: AdaptiveThresholdTypes.GaussianC,
                    thresholdType: ThresholdTypes.Binary,
                    blockSize: (gray.Height / 2) | 1,
                    c: 0.0);
                //threshWin.ShowImage(thresh);

                var found = Cv2.FindChessboardCorners(thresh, patternSize, out corners, ChessboardFlags.None); //, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);

                //frame.CopyTo(output);
                //Cv2.DrawChessboardCorners(output, patternSize, corners, found);
                //if (!found) Console.Out.WriteLine("Chessboard not found :( ");

                if (found)
                {
                    var boardPoints = new Point2d[21];
                    var foundPoints = corners.Select(c => new Point2d((double)c.X, (double)c.Y)).ToArray();
                    for (int c = 0; c < 7; c++)
                    {
                        for (int r = 0; r < 3; r++)
                        {
                            boardPoints[r * 7 + c] = new Point2d((c + 1.0), (r + 3.0));
                        }
                    }

                    var boardToImageTransform = Cv2.FindHomography(boardPoints, foundPoints);
                    var imageToBoardTransform = boardToImageTransform.Inv(); //Cv2.FindHomography(foundPoints, boardPoints);
                    model = new ChessboardModel(boardToImageTransform, imageToBoardTransform, frame);
                }
            } while (frame != null && model == null);

            return model;
        }
    }

    class ChessboardModel
    {
        public Mat BoardToImageTransform { get; }
        public Mat ImageToBoardTransform { get; }
        public Scalar WhiteSquareColor { get; }
        public Scalar BlackSquareColor { get; }
        public Mat BackgroundModel { get; private set; }

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
                for (int row = 2; row < 6; row++)
                {
                    var color = (row + col) % 2 == 0 ? BlackSquareColor : WhiteSquareColor;
                    var squarePoints = GetSquarePoints(row, col).Select(p => new Point(p.X, p.Y));
                    Cv2.FillConvexPoly(result, squarePoints, color);
                }
            }
            return result;
        }

        public Point2d[] GetSquarePoints(int row, int col)
        {
            return GetSquarePoints(row, col, BoardToImageTransform);
        }

        public static Point2d[] GetSquarePoints(int row, int col, Mat boardToImageTransform)
        {
            return Cv2.PerspectiveTransform(
                new Point2d[]
                {
                                new Point2d((double) row, (double) col),
                                new Point2d((double) row, (double) col + 1),
                                new Point2d((double) row + 1, (double) col + 1),
                                new Point2d((double) row + 1, (double) col)
                },
                boardToImageTransform);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="dest">Destination matrix to hold the mask; must be the same size as the video frames used to initialize the model.</param>
        /// <returns></returns>
        public void GetSquareMask(int row, int col, Mat dest)
        {
            GetSquareMask(row, col, dest, BoardToImageTransform);
        }

        public static void GetSquareMask(int row, int col, Mat dest, Mat boardToImageTransform)
        {
            var points = GetSquarePoints(row, col, boardToImageTransform).Select(p => new Point(p.X, p.Y));
            Cv2.FillConvexPoly(dest, points, Scalar.White);
        }
    }
}
