using System;

namespace Alligator.UltimateTicTacToe.Solver
{
    public interface IHashing
    {
        ulong HashCode { get; }
        void Modify(params int[] indices);
    }
}
