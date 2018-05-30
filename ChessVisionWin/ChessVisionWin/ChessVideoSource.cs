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

        public ChessVideoSource(VideoCapture video)
        {
            this.videoCapture = video ?? throw new ArgumentNullException(nameof(video));
            frame = new Mat(new[] { video.FrameWidth, video.FrameHeight }, MatType.CV_8UC3);
        }

        public Mat GetNextFrame()
        {
            if (videoCapture.Read(frame) && frame.Width + frame.Height > 0)
            {
                // Resize to 640xN frames
                double sizeFactor = 640.0 / frame.Width;
                Size newSize = new Size(frame.Width * sizeFactor, frame.Height * sizeFactor);
                Cv2.Resize(frame, frame, newSize);
                return frame;
            }
            else
            {
                return null;
            }
        }
    }
}
