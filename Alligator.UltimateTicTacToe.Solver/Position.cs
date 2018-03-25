using Alligator.Solver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alligator.UltimateTicTacToe.Solver
{
    public class Position : IPosition<Cell>
    {
        public struct WinningChance
        {
            public double Own;
            public double Opp;

            public WinningChance (double own, double opp)
	        {
                Own = own;
                Opp = opp;
	        }

            public override string ToString()
            {
                return string.Format("P[{0}#{1}]", Math.Round(Own, 3), Math.Round(Opp, 3));
            }
        }

        private bool hasWinner;
        private IList<bool> isQuiet;

        private readonly Mark[] combinedBoard;
        private readonly Mark[][] innerBoards;
        private readonly IList<Cell> history;
        private Mark nextMarkType;

        private const int HashParamsLength = 2 * 81 + 4;
        private readonly IHashing hashing = new ZobristHashing(HashParamsLength);

        public WinningChance[] innerWinningChances;
        private int score;

        private static readonly int[][] lines = new int[][]
        {
            new []{ 0, 1, 2 },
            new []{ 3, 4, 5 },
            new []{ 6, 7, 8 },
            new []{ 0, 3, 6 },
            new []{ 1, 4, 7 },
            new []{ 2, 5, 8 },         
            new []{ 0, 4, 8 },
            new []{ 2, 4, 6 }
        };

        public Position()
        {
            hasWinner = false;
            isQuiet = new List<bool> { true };

            innerBoards = new Mark[9][];
            for (int i = 0; i < 9; i++)
            {
                innerBoards[i] = new Mark[9];
            }
            history = new List<Cell>();
            nextMarkType = Mark.X;

            combinedBoard = new Mark[9];

            innerWinningChances = Enumerable.Range(0, 9).Select(t => ComputeWinningChance(t)).ToArray();
            score = 0;
        }

        public Position(IEnumerable<Cell> history)
            : this()
        {
            foreach (var ply in history)
            {
                Do(ply);
            }
        }

        public ulong Identifier
        {
            get { return hashing.HashCode + (history.Count == 0 ? 0ul : (ulong)(history[history.Count - 1].CellIndex)); }
        }

        public bool IsEnded
        {
            get { return hasWinner || IsFullTable(); }
        }

        private bool IsFullTable()
        {
            for (int i = 0; i < 9; i++)
            {
                if (combinedBoard[i] != Mark.Empty)
                {
                    continue;
                }
                if (innerBoards[i].Any(t => t == Mark.Empty))
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasWinner
        {
            get { return hasWinner; }
        }

        public bool IsQuiet
        {
            get { return isQuiet[isQuiet.Count - 1]; }
        }

        public Mark[] CombinedBoard
        {
            get { return combinedBoard; }
        }

        public IList<Cell> History
        {
            get { return history; }
        }

        public int Score
        {
            get { return score; }
        }

        public void Do(Cell ply)
        {
            if (ply == null)
            {
                throw new ArgumentNullException("ply");
            }
            if (hasWinner)
            {
                throw new InvalidOperationException(string.Format("Position has winner, but the game isn't ended"));
            }
            if (innerBoards[ply.BoardIndex][ply.CellIndex] != Mark.Empty)
            {
                throw new InvalidOperationException(string.Format("Cannot mark, because target cell isn't empty: [{0},{1}]",
                    ply.BoardIndex, ply.CellIndex));
            }
            if (history.Count > 0 && combinedBoard[history[history.Count - 1].CellIndex] == Mark.Empty && innerBoards[history[history.Count - 1].CellIndex].Any(t => t == Mark.Empty) && ply.BoardIndex != history[history.Count - 1].CellIndex)
            {
                throw new InvalidOperationException(string.Format("Invalid target board: [{0},{1}]",
                    ply.BoardIndex, ply.CellIndex));
            }
            if (combinedBoard[ply.BoardIndex] != Mark.Empty)
            {
                throw new InvalidOperationException(string.Format("Closed target board: [{0},{1}]",
                    ply.BoardIndex, ply.CellIndex));
            }

            innerBoards[ply.BoardIndex][ply.CellIndex] = nextMarkType;

            history.Add(ply);
            Update(ply);
            hashing.Modify(GetHashIndex(ply));
            nextMarkType = ChangeMark(nextMarkType);
            innerWinningChances[ply.BoardIndex] = ComputeWinningChance(ply.BoardIndex);
            score = CalculateScore();
        }

        public void Undo()
        {
            if (history.Count == 0)
            {
                throw new InvalidOperationException("Cannot remove mark from empty board");
            }
            var lastPly = history[history.Count - 1];
            if (innerBoards[lastPly.BoardIndex][lastPly.CellIndex] == Mark.Empty)
            {
                throw new InvalidOperationException(string.Format("Cannot remove mark, because target cell is already empty: [{0},{1}]",
                    lastPly.BoardIndex, lastPly.CellIndex));
            }
            innerBoards[lastPly.BoardIndex][lastPly.CellIndex] = Mark.Empty;
            combinedBoard[lastPly.BoardIndex] = Mark.Empty;
            history.RemoveAt(history.Count - 1);
            hasWinner = false;
            nextMarkType = ChangeMark(nextMarkType);
            hashing.Modify(GetHashIndex(lastPly));
            innerWinningChances[lastPly.BoardIndex] = ComputeWinningChance(lastPly.BoardIndex);
            score = CalculateScore();
            isQuiet.RemoveAt(isQuiet.Count - 1);
        }

        public IEnumerable<Cell> EnumerateStrategies()
        {
            IList<Cell> result = new List<Cell>();

            if (history.Count == 0)
            {
                result.Add(new Cell(4, 4));
                result.Add(new Cell(4, 3));
                result.Add(new Cell(4, 0));
            }
            else
            {
                var index = history[history.Count - 1].CellIndex;

                if (combinedBoard[index] == Mark.Empty)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (innerBoards[index][i] == Mark.Empty)
                        {
                            result.Add(new Cell(index, i));
                        }
                    }
                }
                if (result.Count == 0)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (combinedBoard[i] != Mark.Empty)
                        {
                            continue;
                        }
                        for (int j = 0; j < 9; j++)
                        {
                            if (innerBoards[i][j] == Mark.Empty)
                            {
                                result.Add(new Cell(i, j));
                            }
                        }
                    }
                }
            }

            return result;
        }

        public Mark GetMarkAt(int boardIndex, int cellIndex)
        {
            return innerBoards[boardIndex][cellIndex];
        }

        private int NextBoardIndex()
        {
            if (history.Count == 0)
            {
                return -1;
            }
            var index = history[history.Count - 1].CellIndex;
            return combinedBoard[index] == Mark.Empty ? index : -1;
        }

        private int GetHashIndex(Cell ply)
        {
            int hash = 9 * ply.BoardIndex + ply.CellIndex;
            return nextMarkType == Mark.X ? hash : 81 + hash;
        }

        private bool HasWinner2(Mark[] board)
        {
            foreach (var line in lines)
            {
                if (board[line[0]] == Mark.Empty)
                {
                    continue;
                }
                if (board[line[0]] != board[line[1]] || board[line[1]] != board[line[2]])
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        private void Update(Cell ply)
        {
            var hasPartialWinner = HasWinner2(innerBoards[ply.BoardIndex]);

            if (hasPartialWinner)
            {
                combinedBoard[ply.BoardIndex] = nextMarkType;
                isQuiet.Add(false);
            }
            else
            {
                isQuiet.Add(true);
            }
            hasWinner = HasWinner2(combinedBoard);
        }

        private Mark ChangeMark(Mark nextMarkType)
        {
            if (nextMarkType == Mark.Empty)
            {
                throw new ArgumentOutOfRangeException("Cannot change empty mark type");
            }
            return nextMarkType == Mark.X ? Mark.O : Mark.X;
        }

        public int CalculateScore()
        {
            double own = 0.0;
            double opp = 0.0;

            foreach (var line in lines)
            {
                own += innerWinningChances[line[0]].Own * innerWinningChances[line[1]].Own * innerWinningChances[line[2]].Own;
                opp += innerWinningChances[line[0]].Opp * innerWinningChances[line[1]].Opp * innerWinningChances[line[2]].Opp;       
            }

            return (int)(10000 * own - 10000 * opp);
        }

        private WinningChance ComputeWinningChance(int index)
        {
            if (combinedBoard[index] == Mark.X)
            {
                return new WinningChance(1.0, 0.0);
            }
            else if (combinedBoard[index] == Mark.O)
            {
                return new WinningChance(0.0, 1.0);
            }

            int ownScore = 0;
            int oppScore = 0;

            foreach (var line in lines)
            {
                int own = 0;
                int opp = 0;

                foreach (var cell in line)
                {
                    switch (innerBoards[index][cell])
                    {
                        case Mark.Empty:
                            break;
                        case Mark.X:
                            own++;
                            break;
                        case Mark.O:
                            opp++;
                            break;
                        default:
                            break;
                    }  
                }
                if (own == 0 && opp == 0)
                {
                    ownScore += 1;
                    oppScore += 1;
                }
                else if (own > 0 && opp > 0)
                {
                    continue;
                }
                else if (own == 2)
                {
                    ownScore += 400;
                }
                else if (opp == 2)
                {
                    oppScore += 400;
                }
                else if (own == 1)
                {
                    ownScore += 50;
                }
                else if (opp == 1)
                {
                    oppScore += 50;
                }
            }

            if (ownScore == 0 && oppScore == 0)
            {
                return new WinningChance(0.0, 0.0);
            }

            var res = 0.5 + (ownScore - oppScore) / 2500.0;

            if (res > 1)
            {
                res = 1.0;
            }
            if (res < 0)
            {
                res = 0.0;
            }
            return new WinningChance(res, 1 - res);
        }
    }
}
