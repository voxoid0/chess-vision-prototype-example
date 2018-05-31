using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ChessVisionWin
{
    class ChessVideoSource : IVideo
    {
        private readonly VideoCapture videoCapture;
        private Mat frame;
        private Window inputWin;
        private double _speed;

        public ChessVideoSource(VideoCapture video, double speed = 1.0)
        {
            this.videoCapture = video ?? throw new ArgumentNullException(nameof(video));
            _speed = speed;
            if (video.FrameWidth == 0) throw new InvalidOperationException("Video source invalid (missing file?)");
            frame = new Mat(new[] { video.FrameWidth, video.FrameHeight }, MatType.CV_8UC3);
            inputWin = new Window("Input");
        }

        public Mat GetNextFrame()
        {
            // Wait for frame realtime, and check for key (exit)
            if (Cv2.WaitKey((int) (1000.0 / videoCapture.Fps / _speed)) != -1) return null;

            if (videoCapture.Read(frame) && frame.Width + frame.Height > 0)
            {
                // Resize to 640xN frames
                double sizeFactor = 640.0 / frame.Width;
                Size newSize = new Size(frame.Width * sizeFactor, frame.Height * sizeFactor);
                Cv2.Resize(frame, frame, newSize);
                inputWin.ShowImage(frame);
                return frame;
            }
            else
            {
                return null;
            }
        }
    }
}
