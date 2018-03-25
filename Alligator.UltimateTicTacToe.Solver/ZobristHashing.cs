using System;

namespace Alligator.UltimateTicTacToe.Solver
{
    public class ZobristHashing : IHashing
    {
        private static ulong[] randomULongs;
        private ulong hashValue;

        private static readonly Random random =
#if DEBUG
 new Random(0);
#else
            new Random();
#endif

        public ZobristHashing(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException("Size must be positive!", "size");
            }
            if (randomULongs == null)
            {
                randomULongs = new ulong[size];
                for (int i = 0; i < size; i++)
                {
                    randomULongs[i] = NextRandomULong();
                }
            }
        }

        public ulong HashCode
        {
            get { return hashValue; }
        }

        public void Modify(params int[] indices)
        {
            foreach (var i in indices)
            {
                hashValue ^= randomULongs[i];
            }
        }

        private ulong NextRandomULong()
        {
            byte[] buffer = new byte[8];
            random.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}
