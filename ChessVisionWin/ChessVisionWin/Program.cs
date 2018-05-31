using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.XPhoto;

namespace ChessVisionWin
{
    class Program
    {
        //private const string VideoPath = @"C:\joel\large\cv-videos\chess\WIN_20180519_20_09_12_Pro.mp4";
        //private const string VideoPath = @"C:\joel\large\cv-videos\chess\WIN_20180519_20_38_04_Pro.mp4";
        //private const string VideoPath = @"C:\code\cv\chess\recordings\MVI_0018.MOV";
        //private const string VideoPath = @"C:\code\cv\chess\recordings\Aronian-Kramnik-2018.MOV";
        //private const string VideoPath = @"C:\joel\large\cv-videos\chess\Aronian-Kramnik-2018.MOV";
        //private const string VideoPath = @"C:\joel\large\cv-videos\chess\Karjakin-Caruana-2018.MOV";
        private const string VideoPath = @"C:\joel\large\cv-videos\chess\Karjakin-Caruana-2018-B.MOV";
        private const string ImagePath = @"C:\code\cv\chess\recordings\cg\chess2.png";

        private Mat resized = new Mat();
        private Mat thresh = new Mat();
        private Window threshWin;
        private Mat gray = new Mat();

        static void Main(string[] args)
        {
            new Program().Run();
        }

        void Run()
        {
            //using (var image = Cv2.ImRead(ImagePath))
            //{
            //    using (var imageWin = new Window("Image", image))
            //    {
            //        using (threshWin = new Window("Threshold", image))
            //        {
            //            var result = new Mat();
            //            ProcessFrame(image, result);
            //            imageWin.ShowImage(result);
            //            Cv2.WaitKey();
            //        }
            //    }
            //}


            using (var videoCapture = new VideoCapture(VideoPath))
            {
                SkipFrames(videoCapture, 3.0);
                var video = new ChessVideoSource(videoCapture, speed: 3.0);
                var chessboardModel = new FgSegChessboardInitializer().Do(video);
                
                var bgModelWin = new Window("Background Model");
                bgModelWin.ShowImage(chessboardModel.BackgroundModel);
                var squaresMaskWin = new Window("Squares Mask");
                squaresMaskWin.ShowImage(chessboardModel.SquaresMask);
                Cv2.WaitKey();

                var segmenter = new FgSegPieceSegmenter(chessboardModel);
                segmenter.Do(video);

                //Cv2.WaitKey();


                //    var frame = new Mat(new[] { video.FrameWidth, video.FrameHeight }, MatType.CV_8UC3);
                //    thresh = new Mat();// frame.Clone();
                //    var output = new Mat();
                //    var nextFrameTime = DateTime.Now;
                //    using (var inputWin = new Window("Input", frame))
                //    {
                //        using (threshWin = new Window("Threshold", frame))
                //        {
                //            //inputWin.DisplayOverlay("Input", 0);
                //            while (video.Read(frame))
                //            {

                //                //var now = DateTime.Now;
                //                video.Read(frame);
                //                if (frame.Width == 0) break;

                //                ProcessFrame(frame, output);
                //                inputWin.ShowImage(output);

                //                if (Cv2.WaitKey((int)(1000.0 / video.Fps)) != -1) break;
                //            }
                //        }
                //    }

            }
        }

        private void SkipFrames(VideoCapture videoCapture, double seconds)
        {
            Mat frame = new Mat();
            for (int f = 0; f < (int) (videoCapture.Fps * seconds); f++)
            {
                videoCapture.Read(frame);
            }
        }


        void ProcessFrame(Mat frame, Mat output)
        {
            double sizeFactor = 640.0 / frame.Width;
            Size newSize = new Size(frame.Width * sizeFactor, frame.Height * sizeFactor);
            Cv2.Resize(frame, frame, newSize);

            //Cv2.Canny(frame, output, 10.0, 130.0, apertureSize: 3, L2gradient: true);


            var corners = new Point2f[4];
            var patternSize = new Size(7, 3);

            //var thresh = new Mat();
            //thresh = new Mat(frame.);
            //Cv2.EdgePreservingFilter(frame, frame, EdgePreservingMethods.RecursFilter, sigmaS: 60, sigmaR: 0.5f);
            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

            //Cv2.MedianBlur(gray, gray, 5); // Disallows chessboard to be found, because of how it opens/closes corners

            ////Cv2.GaussianBlur(gray, gray, new Size(15, 15), 0, 0);
            Cv2.AdaptiveThreshold(gray, thresh,
                maxValue: 255.0,
                adaptiveMethod: AdaptiveThresholdTypes.GaussianC,
                thresholdType: ThresholdTypes.Binary,
                blockSize: (gray.Height / 2) | 1,
                c: 0.0);
            //Cv2.Canny(gray, thresh, 48, 96, apertureSize: 3, L2gradient: true); // with Sobel op
            //LineSegmentPolar[] lines = Cv2.HoughLines(thresh, 4.0, Math.PI * 2.0 / 360.0, frame.Height / 2);
            //Console.WriteLine($"{lines.Length} lines found.");

            //double threshold = Cv2.Mean(gray).Val0;
            //Cv2.Threshold(gray, thresh, threshold, 255.0, ThresholdTypes.Binary);

            threshWin.ShowImage(thresh);

            var found = Cv2.FindChessboardCorners(thresh, patternSize, out corners, ChessboardFlags.None); //, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);
            //var found = Cv2.FindChessboardCorners(frame, patternSize, out corners, ChessboardFlags.None); //, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);

            frame.CopyTo(output);
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

                var boardToImageTransform = Cv2.FindHomography(boardPoints, foundPoints);
                var imageToBoardTransform = boardToImageTransform.Inv(); //Cv2.FindHomography(foundPoints, boardPoints);
                //imageToBoardTransform.Set(0, 0, imageToBoardTransform.Get<double>(0, 0) / 40.0);
                //imageToBoardTransform.Set(1, 1, imageToBoardTransform.Get<double>(1, 1) / 40.0);
                //imageToBoardTransform.Set(2, 2, imageToBoardTransform.Get<double>(2, 2) / 40.0);
                //Cv2.PerspectiveTransform()

                //var xformedFound = Cv2.Transform()
                //var imageXForm = boardToWorldTransform
                //Console.Out.WriteLine(boardToWorldTransform);

                //Cv2.WarpPerspective(frame, output, imageToBoardTransform, frame.Size(), InterpolationFlags.Cubic, BorderTypes.Constant, Scalar.Black);
                for (int i = 0; i < 8; i++)
                {
                    DrawCellPoly(boardToImageTransform, output, i, i);
                }
            }
        }

        static void DrawCellPoly(Mat boardToImageTransform, Mat output, int row, int col)
        {
            Point2d[] corners = Cv2.PerspectiveTransform(
                new Point2d[]
                {
                    new Point2d((double) row, (double) col),
                    new Point2d((double) row, (double) col + 1),
                    new Point2d((double) row + 1, (double) col + 1),
                    new Point2d((double) row + 1, (double) col)
                }, 
                boardToImageTransform);
            Cv2.FillConvexPoly(output, corners.Select(p => new Point(p.X, p.Y)), Scalar.OrangeRed, LineTypes.AntiAlias);
        }



    }
}
