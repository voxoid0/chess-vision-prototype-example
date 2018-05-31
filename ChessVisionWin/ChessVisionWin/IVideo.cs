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
}