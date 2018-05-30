using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ChessVisionWin
{
    public class DefaultPipelineStage : IPipelineStage
    {
        public string Name { get; }

        public Mat CurrentImage { get; }

        public IReadOnlyList<IStageParam> Parameters { get; }


        public DefaultPipelineStage(string name, IReadOnlyList<IStageParam> parameters)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
            CurrentImage = new Mat();
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }
    }
}
