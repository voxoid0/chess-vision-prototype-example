using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.XPhoto;

namespace BasicImageProc
{
    class Program
    {
        private const string ImagePath = @"C:\code\cv\chess\recordings\ak18-1.png";

        private Mat resized = new Mat();
        private Mat thresh = new Mat();
        private Window threshWin;
        private Mat gray = new Mat();

        static void Main(string[] args)
        {
            new Program().Run();
        }

        void Run() {
            var input = Cv2.ImRead(ImagePath);
            Cv2.Resize(input, input, new Size(input.Width / 2, input.Height / 2));
            var grayscale = new Mat(input.Size(), MatType.CV_8UC1);
            Cv2.CvtColor(input, grayscale, ColorConversionCodes.BGR2GRAY);
            var imageWin = new Window("Input", grayscale);
            imageWin.ShowImage(grayscale);
            Cv2.ImWrite("input.png", grayscale);

            // Process
            var result = new Mat();

            //Cv2.Threshold(grayscale, result, 64, 255, ThresholdTypes.Binary); // 64, 127, 100

            //Cv2.Threshold(grayscale, grayscale, 100, 255, ThresholdTypes.Binary);
            //Cv2.Erode(grayscale, result, new Mat()); // 3x3

            //Cv2.Threshold(grayscale, grayscale, 100, 255, ThresholdTypes.Binary);
            //Cv2.Dilate(grayscale, result, new Mat());

            //Cv2.GaussianBlur(grayscale, result, new Size(5, 5), 0, 0);

            // TODO: use noisy image, e.g. low lighting
            //imageWin.ShowImage(input);
            //Cv2.EdgePreservingFilter(input, result, EdgePreservingMethods.RecursFilter, sigmaS: 60f, sigmaR: 0.4); // ???

            // Harris
            //Point2f[] features = Cv2.GoodFeaturesToTrack(grayscale, maxCorners: 64, qualityLevel: 0.01, minDistance: 5, mask: new Mat(), blockSize: 7, useHarrisDetector: true, k: 0.05);
            //result = input.Clone();
            //DrawPoints(features, result, Scalar.Red);

            // FAST
            KeyPoint[] keyPoints = Cv2.FAST(grayscale, 32, nonmaxSupression: true);
            Point2f[] features = keyPoints.Select(kp => kp.Pt).ToArray();
            result = input.Clone();
            DrawPoints(features, result, Scalar.Red);

            //Point2f[] features = Cv2.Fast(grayscale, maxCorners: 64, qualityLevel: 0.01, minDistance: 5, mask: new Mat(), blockSize: 7, useHarrisDetector: true, k: 0.05);
            //result = input.Clone();
            //DrawPoints(features, result, Scalar.Red);

            

            var outputWin = new Window("Output", result);
            outputWin.ShowImage(result);
            Cv2.ImWrite("output.png", result);
            Cv2.WaitKey();
        }

        private void DrawPoints(Point2f[] points, Mat image, Scalar color)
        {
            foreach (Point2f point in points)
            {
                Cv2.Circle(image, point, 2, color, thickness: 1);
            }
        }
    }
}
