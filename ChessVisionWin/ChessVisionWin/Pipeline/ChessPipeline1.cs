using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessVisionWin
{
    class ChessPipeline1 : IPipeline
    {
        public IReadOnlyList<IPipelineStage> Stages { get; }
    }
}
