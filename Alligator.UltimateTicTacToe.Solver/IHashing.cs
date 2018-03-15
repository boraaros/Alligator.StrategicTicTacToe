using System;

namespace Alligator.StrategicTicTacToe.Solver
{
    public interface IHashing
    {
        ulong HashCode { get; }
        void Modify(params int[] indices);
    }
}
