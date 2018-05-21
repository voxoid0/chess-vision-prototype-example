using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace ChessVisionWin
{
    static class Program
    {
        //private const string VideoPath = @"C:\joel\large\cv-videos\chess\WIN_20180519_20_09_12_Pro.mp4";
        private const string VideoPath = @"C:\joel\large\cv-videos\chess\WIN_20180519_20_38_04_Pro.mp4";
        private const string ImagePath = @"C:\code\cv\chess\recordings\cg\chess2.png";

        static void Main(string[] args)
        {
            using (var image = Cv2.ImRead(ImagePath))
            {
                using (var imageWin = new Window("Image", image))
                {
                    var result = new Mat();
                    ProcessFrame(image, result);
                    imageWin.ShowImage(result);
                    Cv2.WaitKey();
                }
            }

            using (var video = new VideoCapture(VideoPath))
            {
                var frame = new Mat(new[] { video.FrameWidth, video.FrameHeight }, MatType.CV_8UC3);
                var output = new Mat();
                var nextFrameTime = DateTime.Now;
                using (var inputWin = new Window("Input", frame))
                {
                    //inputWin.DisplayOverlay("Input", 0);
                    while (video.Read(frame))
                    {
                        //var now = DateTime.Now;
                        video.Read(frame);
                        if (frame.Width == 0) break;

                        ProcessFrame(frame, output);
                        inputWin.ShowImage(output);
                        
                        if (Cv2.WaitKey((int) (1000.0 / video.Fps)) != -1) break;
                    }
                }

            }
        }
        static void ProcessFrame(Mat frame, Mat output)
        {
            //Cv2.Canny(frame, output, 10.0, 130.0, apertureSize: 3, L2gradient: true);
            frame.CopyTo(output);

            var corners = new Point2f[4];
            var patternSize = new Size(7, 3);
            var found = Cv2.FindChessboardCorners(frame, patternSize, out corners, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);
            Cv2.DrawChessboardCorners(output, patternSize, corners, found);
            if (!found) Console.Out.WriteLine("Chessboard not found :( ");

            if (found)
            {
                var boardPoints = new Point2d[21];
                var foundPoints = corners.Select(c => new Point2d((double) c.X, (double) c.Y)).ToArray();
                for (int c = 0; c < 7; c++)
                {
                    for (int r = 0; r < 3; r++)
                    {
                        boardPoints[r * 7 + c] = new Point2d((c + 1.0), (r + 3.0));
                    }
                }

                var boardToWorldTransform = Cv2.FindHomography(boardPoints, foundPoints);
                var worldToBoardTransform = boardToWorldTransform.Inv(); //Cv2.FindHomography(foundPoints, boardPoints);
                worldToBoardTransform.Set(0, 0, worldToBoardTransform.Get<double>(0, 0) / 40.0);
                worldToBoardTransform.Set(1, 1, worldToBoardTransform.Get<double>(1, 1) / 40.0);
                worldToBoardTransform.Set(2, 2, worldToBoardTransform.Get<double>(2, 2) / 40.0);

                //var xformedFound = Cv2.Transform()
                //var imageXForm = boardToWorldTransform
                Cv2.WarpPerspective(frame, output, worldToBoardTransform, frame.Size(), InterpolationFlags.Cubic, BorderTypes.Constant, Scalar.Aqua);
                //Console.Out.WriteLine(boardToWorldTransform);
            }
        }
    }
}
