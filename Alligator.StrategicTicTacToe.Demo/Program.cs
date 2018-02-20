using Alligator.Solver;
using Alligator.StrategicTicTacToe.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alligator.StrategicTicTacToe.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Hello strategic tic-tac-toe demo!");

            var externalLogics = new Logics();
            var solverConfiguration = new SolverConfiguration();
            var solverFactory = new SolverFactory<Position, Cell>(externalLogics, solverConfiguration);
            ISolver<Cell> solver = solverFactory.Create();

            Position position = new Position();
            IList<Cell> history = new List<Cell>();
            bool aiStep = true;

            while (!position.IsEnded)
            {
                PrintPosition(position);
                Cell next;
                Position copy = new Position(position.History);

                if (aiStep)
                {
                    while (true)
                    {
                        try
                        {
                            solver = solverFactory.Create();
                            next = AiStep(history, solver);
                            copy.Do(next);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                else
                {
                    while (true)
                    {
                        try
                        {
                            next = HumanStep();
                            copy.Do(next);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                position.Do(next);
                history.Add(next);
                aiStep = !aiStep;
            }
            if (!position.HasWinner)
            {
                Console.WriteLine("Game over, DRAW!");
            }
            else
            {
                Console.WriteLine(string.Format("Game over, {0} WON!", aiStep ? "human" : "ai"));
            }

            PrintPosition(position);

            Console.ReadKey();
        }

        private static Cell HumanStep()
        {
            Console.Write("Next step [row:column]: ");
            while (true)
            {
                try
                {
                    string[] msg = Console.ReadLine().Split(':');
                    var x = 3 * (int.Parse(msg[0]) / 3) + int.Parse(msg[1]) / 3;
                    var y = 3 * (int.Parse(msg[0]) % 3) + int.Parse(msg[1]) % 3;
                    return new Cell(x, y);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static Cell AiStep(IList<Cell> history, ISolver<Cell> solver)
        {
            Position position = new Position(history);

            IList<Cell> forecast;
            int evaluationValue = solver.Maximize(history, out forecast);

            if (forecast == null || forecast.Count == 0)
            {
                throw new InvalidOperationException("Solver error!");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Alligator is thinking...");
            Console.WriteLine(string.Format("Evaluation value: {0} ({1})", evaluationValue, ToString(evaluationValue)));
            Console.WriteLine(string.Format("Optimal next step: {0}", forecast[0]));
            Console.WriteLine(string.Format("Forecast: {0}", string.Join(" --> ", forecast)));
            Console.ForegroundColor = ConsoleColor.White;

            return forecast[0];
        }

        private static string ToString(int evaluationValue)
        {
            if (evaluationValue > 11000)
            {
                return "It's time to give up? ;-)";
            }
            if (evaluationValue > 7000)
            {
                return "Ho-Ho-Ho!!!";
            }
            if (evaluationValue > 3000)
            {
                return "Not bad, not bad.. for me!";
            }
            if (evaluationValue < -5000)
            {
                return "Oh, no!";
            }
            else if (evaluationValue < -2000)
            {
                return "You are good!";
            }
            return "Hm, draw..";
        }

        private static void PrintPosition(Position position)
        {
            var validSteps = position.EnumerateStrategies().ToList();

            Console.WriteLine(string.Join("-", Enumerable.Range(0, 13).Select(t => "=")));
            Console.WriteLine("  0 1 2 | 3 4 5 | 6 7 8 ");

            for (int i = 0; i < 9; i++)
            {
                Console.Write(i);

                for (int j = 0; j < 9; j++)
                {
                    var x = 3 * (i / 3) + j / 3;
                    var y = 3 * (i % 3) + j % 3;

                    var isValidNext = validSteps.Contains(new Cell(x, y));

                    switch (position.CombinedBoard[x] == Mark.Empty ? position.GetMarkAt(x, y) : position.CombinedBoard[x])
                    {
                        case Mark.Empty:
                            if (isValidNext)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            Console.Write(string.Format(" {0}", "."));
                            break;
                        case Mark.X:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(string.Format(" {0}", Mark.X));
                            break;
                        case Mark.O:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(string.Format(" {0}", Mark.O));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    if (j == 2 || j == 5)
                    {
                        Console.Write(" |");
                    }
                }
                Console.WriteLine();
                if (i == 2 || i == 5)
                {
                    Console.WriteLine("-----------------------");
                }
            }
            Console.WriteLine(string.Join("-", Enumerable.Range(0, 13).Select(t => "=")));
        }
    }
}
