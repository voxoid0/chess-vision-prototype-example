namespace ChessVisionWin
{
    class SquareClassificationScores
    {
        private readonly double[] scores;

        public SquareClassificationScores(double[] scores)
        {
            this.scores = scores;
        }

        public SquareState MostLikely
        {
            get
            {
                if (White >= Black && White > Empty)
                    return SquareState.White;
                else if (Black >= White && Black > Empty)
                    return SquareState.Black;
                else
                    return SquareState.Empty;
            }
        }
            
        public double Empty => scores[(int) SquareState.Empty];
        public double White => scores[(int)SquareState.White];
        public double Black => scores[(int)SquareState.Black];
    }
}