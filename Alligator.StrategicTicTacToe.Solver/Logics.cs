using Alligator.Solver;
using System;
using System.Collections.Generic;

namespace Alligator.StrategicTicTacToe.Solver
{
    public class Logics : IExternalLogics<Position, Cell>
    {
        public bool IsInverted;

        public Position CreateEmptyPosition()
        {
            return new Position();
        }

        public IEnumerable<Cell> GetStrategiesFrom(Position position)
        {
            return position.EnumerateStrategies();
        }

        public int StaticEvaluate(Position position)
        {
            return IsInverted ? -position.Score : position.Score;
        }
    }
}
