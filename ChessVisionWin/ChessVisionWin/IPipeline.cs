using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessVisionWin
{
    public interface IPipeline
    {
        //IReadOnlyList<string> StageNames { get; }
        //IPipelineStage GetStage(string name);
        IReadOnlyList<IPipelineStage> Stages { get; }
    }
}
