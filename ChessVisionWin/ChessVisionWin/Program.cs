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

        static void Main(string[] args)
        {
            using (var video = new VideoCapture(VideoPath))
            {
                var frame = new Mat(new[] { video.FrameWidth, video.FrameHeight }, MatType.CV_8UC3);
                var output = new Mat();
                var nextFrameTime = DateTime.Now;
                using (var inputWin = new Window("Input", frame))
                {
                    inputWin.DisplayOverlay("Input", 0);
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
            Cv2.Canny(frame, output, 10.0, 130.0);
        }
    }
}
