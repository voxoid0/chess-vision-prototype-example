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
        //private const string VideoPath = @"C:\joel\large\cv-videos\chess\Karjakin-Caruana-2018.MOV";

        private const string VideoPath = @"C:\joel\large\cv-videos\chess\Aronian-Kramnik-2018.MOV";  // Starts in initial positions
        //private const string VideoPath = @"C:\joel\large\cv-videos\chess\Karjakin-Caruana-2018-B.MOV"; // Starts with empty board


        private Window threshWin;


        static void Main(string[] args)
        {
            new Program().Run();
        }

        void Run()
        {
            using (var videoCapture = new VideoCapture(VideoPath))
            {
                SkipFrames(videoCapture, 3.0); // Allow most camera's initial lighting changes to pass
                var video = new ChessVideoSource(videoCapture, speed: 3.0);
                //var chessboardModel = new FgSegChessboardInitializer().Do(video);
                var chessboardModel = new HistChessboardInitializer().Do(video);
                

                //var segmenter = new FgSegPieceSegmenter(chessboardModel);
                var segmenter = new HistogramPieceSegmenter(chessboardModel);
                segmenter.Do(video);
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
    }
}
