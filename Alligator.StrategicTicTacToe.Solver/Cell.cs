using System;

namespace Alligator.StrategicTicTacToe.Solver
{
    public class Cell
    {
        public readonly int BoardIndex;
        public readonly int CellIndex;

        public Cell(int boardIndex, int cellIndex)
        {
            BoardIndex = boardIndex;
            CellIndex = cellIndex;
        }

        public override int GetHashCode()
        {
            return  9 * BoardIndex + CellIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            var ply = obj as Cell;
            if (ply == null)
            {
                return false;
            }
            return BoardIndex == ply.BoardIndex && CellIndex == ply.CellIndex;
        }

        public override string ToString()
        {
            return string.Format("[B#{0}-C#{1}]", BoardIndex, CellIndex);
        }
    }
}
