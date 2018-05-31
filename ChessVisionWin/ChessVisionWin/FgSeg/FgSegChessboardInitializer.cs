using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ChessVisionWin
{
    class FgSegChessboardInitializer
    {
        // TODO: Optional<Mat>
        public FgSegChessboardModel Do(IVideo video)
        {
            FgSegChessboardModel model = null;
            Mat frame;
            Mat gray = new Mat();
            Mat thresh = new Mat();
            var corners = new Point2f[4];
            var patternSize = new Size(7, 7);
            var threshWin = new Window("Adaptive Threshold");

            // TODO: each iteration, try different block sizes for the adaptive threshold (height / 4, height / 2, etc)
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
                        blockSize: (gray.Height / 4) | 1,
                        c: 0.0);
                    threshWin.ShowImage(thresh);

                    var found = Cv2.FindChessboardCorners(thresh, patternSize, out corners,
                        ChessboardFlags.None); //, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);

                    //frame.CopyTo(output);
                    //Cv2.DrawChessboardCorners(output, patternSize, corners, found);
                    //if (!found) Console.Out.WriteLine("Chessboard not found :( ");

                    if (found)
                    {
                        var boardPoints = new Point2d[7*7];
                        Point2d[] foundPoints = OCVUtil.Point2fTo2d(corners);
                        for (int c = 0; c < 7; c++)
                        {
                            for (int r = 0; r < 7; r++)
                            {
                                boardPoints[r * 7 + c] = new Point2d((c + 1.0), (r + 1.0));
                            }
                        }

                        var boardToImageTransform = Cv2.FindHomography(boardPoints, foundPoints);
                        var imageToBoardTransform = boardToImageTransform.Inv(); //Cv2.FindHomography(foundPoints, boardPoints);
                        model = new FgSegChessboardModel(boardToImageTransform, imageToBoardTransform, frame);
                    }
                }
            } while (frame != null && model == null);

            return model;
        }
    }
}
