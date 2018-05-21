using System.Collections.Generic;
using OpenCvSharp;

namespace ChessVisionWin
{
    public interface IPipelineStage
    {
        string Name { get; }
        Mat CurrentImage { get; }
        IReadOnlyList<IStageParam> Parameters { get; }
    }

    public interface IStageParam
    {
        string Name { get; }
    }

    public class IntStageParam
    {
        public string Name { get; }
        public int Value { get; set; }
        //public event int Value;
        public int Min { get; }
        public int Max { get; }
    }

    public class DoubleStageParam
    {
        public string Name { get; }
        public double Value { get; set; }
        //public event int Value;
        public double Min { get; }
        public double Max { get; }
    }
}