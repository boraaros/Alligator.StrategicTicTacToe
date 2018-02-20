using Alligator.Solver;
using System;

namespace Alligator.StrategicTicTacToe.Demo
{
    class SolverConfiguration : ISolverConfiguration
    {
        public TimeSpan TimeLimitPerMove
        {
            get { return TimeSpan.FromSeconds(3); }
        }

        public int SearchDepthLimit
        {
            get { return 20; }
        }

        public int QuiescenceExtensionLimit
        {
            get { return 0; }
        }

        public int EvaluationTableSizeExponent
        {
            get { return 4; }
        }

        public int EvaluationTableRetryLimit
        {
            get { return 0; }
        }

        public int TranspositionTableSizeExponent
        {
            get { return 24; }
        }

        public int TranspositionTableRetryLimit
        {
            get { return 1; }
        }
    }
}
