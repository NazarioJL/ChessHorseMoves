using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChesssHorse
{
    class Program
    {
        static List<byte>[] ValidSquarePositions = new List<byte>[64];
        List<Stack<byte>> PathsFound = new List<Stack<byte>>();
        static ulong Iterations = 0;

        static bool IsValidStepFromSquare(sbyte Square, Tuple<sbyte, sbyte> Step)
        {
            sbyte col = (sbyte)(Square & 0x7);
            sbyte row = (sbyte)(Square >> 3);

            col += Step.Item1;
            row += Step.Item2;

            return (row >= 0 && row <= 7 && col >= 0 && col <= 7);
        }

        static byte AddStepToSquare(byte Square, Tuple<sbyte, sbyte> Step)
        {
            sbyte col = (sbyte)(Square & 0x7);
            sbyte row = (sbyte)(Square >> 3);

            col += Step.Item1;
            row += Step.Item2;

            return (byte)((row << 3) + col);
        }

        static void CreateValidSquareMoveList(List<byte>[] ValidSquarePositions)
        {
            Tuple<sbyte, sbyte>[] Steps = new Tuple<sbyte, sbyte>[8]
            {
                new Tuple<sbyte, sbyte>(-2, 1),
                new Tuple<sbyte, sbyte>(-2, -1),
                new Tuple<sbyte, sbyte>(2, 1),
                new Tuple<sbyte, sbyte>(2, -1),
                new Tuple<sbyte, sbyte>(-1, 2),
                new Tuple<sbyte, sbyte>(-1, -2),
                new Tuple<sbyte, sbyte>(1, 2),
                new Tuple<sbyte, sbyte>(1, -2)
            };

            for (byte pos = 0; pos < 64; pos++)
            {
                List<byte> ValidPos = new List<byte>();
                foreach (Tuple<sbyte, sbyte> step in Steps)
                {
                    if (IsValidStepFromSquare((sbyte)pos, step))
                    {
                        ValidPos.Add(AddStepToSquare(pos, step));
                    }
                }

                ValidSquarePositions[pos] = ValidPos;
            }

            // Optmization: Sort the list by the number of possible steps (smallest # of steps goes first)
            foreach (List<byte> SqaureSteps in ValidSquarePositions)
            {
                SqaureSteps.Sort(
                    delegate (byte p1, byte p2)
                    {
                        return ValidSquarePositions[p1].Count.CompareTo(ValidSquarePositions[p2].Count);
                    }
                );
            }
        }

        static string SquareToChessPosition(byte Square)
        {
            char[] Name = new char[2];

            Name[0] = (char)(((int)'a') + (Square & 0x7));
            Name[1] = (char)(((int)'1') + (Square >> 3));
            return new string(Name);
        }

        static bool VisitedSquare(byte Square, ulong VisitedMap)
        {
            return 0 < (VisitedMap & (ulong)(((ulong)1) << Square));
        }

        static void MarkSquareVisited(byte CurrentSquare, ref ulong VisitedMap)
        {
            VisitedMap |= (ulong)(((ulong)1) << CurrentSquare);
        }

        static void ClearSquareVisited(byte CurrentSquare, ref ulong VisitedMap)
        {
            VisitedMap &= ~((ulong)(((ulong)1) << CurrentSquare));
        }

        static bool CheckCompletedPath(ulong VisitedMap)
        {
            Iterations++;
            return (VisitedMap == ulong.MaxValue);
        }

        static List<string> ConvertSquareToChessPosition(Stack<byte> Squares)
        {
            List<string> Steps = new List<string>();
            foreach (byte square in Squares)
            {
                Steps.Add(SquareToChessPosition(square));
            }
            return Steps;
        }

        static bool FindPath(byte Square, ulong VisitedMap, Stack<byte> Path, List<Stack<byte>> PathsFound)
        {
            // Find the list of valid positions the horse can jump to from this square 
            // For each valid position the horse can jump to, explore the path.
            List<byte> Positions = ValidSquarePositions[Square];
            foreach (byte CurrentSquare in Positions)
            {
                // Make sure the horse does not jump into a square we already visited.
                if (!VisitedSquare(CurrentSquare, VisitedMap))
                {
                    // Add this square to the current path.
                    Path.Push(CurrentSquare);
                    // Mark the square as visited in the map.
                    MarkSquareVisited(CurrentSquare, ref VisitedMap);
                    // Let's see if we reach a valid path
                    if (CheckCompletedPath(VisitedMap))
                    {
                        // we just found a path. Create a copy of the Path and store it on the list of paths.
                        PathsFound.Add(new Stack<byte>(Path));
#if true
                        return true;
#endif                                         
                    }
                    else
                    {
#if true
                        // Let's continue looking...
                        if (FindPath(CurrentSquare, VisitedMap, Path, PathsFound))
                        {
                            return true;
                        }
#else
                        FindPath(CurrentSquare, VisitedMap, Path, PathsFound);
#endif
                    }

                    // Remove this square to the current path, since we already tested it.
                    Path.Pop();
                    // Mark the square as not visited in the map.
                    ClearSquareVisited(CurrentSquare, ref VisitedMap);
                }
                // else, this square is already visited, let's move on to the next.
            }
            return false;
        }
        static bool ValidatePath(Stack<byte> Moves)
        {
            // First, make sure there exactly 64 moves.
            if (Moves.Count != 64)
            {
                Console.WriteLine("Invalid moves count" + Moves.Count);
                return false;
            }

            // Make sure there are no duplicates
            List<byte> SortedMoves = Moves.ToList();
            SortedMoves.Sort();

            for (byte curr = 0; curr < (Moves.Count - 1); curr++)
            {
                if (SortedMoves.ElementAt(curr) == SortedMoves.ElementAt(curr + 1))
                {
                    Console.WriteLine("Duplicate found: " + SquareToChessPosition(curr));
                    return false;
                }
            }
            // Validates that each move in the sequence is legal.
            Stack<byte> MovesCopy = new Stack<byte>(Moves);
            byte CurrentMove = MovesCopy.Pop();
            while (MovesCopy.Count > 0)
            {
                byte NextMove = MovesCopy.Pop();
                if (!ValidSquarePositions[CurrentMove].Contains(NextMove))
                {
                    Console.WriteLine("Ilegal move: " + SquareToChessPosition(CurrentMove) + " to " + SquareToChessPosition(NextMove));
                    return false;
                }

                CurrentMove = NextMove;
            }

            return true;
        }

        static void Main(string[] args)
        {
            CreateValidSquareMoveList(ValidSquarePositions);
            for (byte Square = 0; Square < 13; Square++)
            {
                Stack<byte> Path = new Stack<byte>();
                List<Stack<byte>> PathsFound = new List<Stack<byte>>();
                byte FirstSquare = Square;
                ulong VisitedMap = 0;
                Iterations = 0;
                Console.WriteLine("Finding path for square: {0}", SquareToChessPosition(FirstSquare));
                // Add this square to the current path.
                Path.Push(FirstSquare);
                // Mark the square as visited in the map.
                MarkSquareVisited(FirstSquare, ref VisitedMap);

                // Create new stopwatch.
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

                // Begin timing.
                stopwatch.Start();

                FindPath(FirstSquare, VisitedMap, Path, PathsFound);

                // Stop timing.
                stopwatch.Stop();
                Console.WriteLine("Time elapsed (ms): {0}", stopwatch.Elapsed.TotalMilliseconds);

                Console.WriteLine("Iterations:" + Iterations);

                foreach (byte Move in Path)
                {
                    Console.Write(SquareToChessPosition(Move) + ",");
                }
                Console.WriteLine("");

                Console.WriteLine("Moves Sequence is valid? " + ValidatePath(Path));
                Console.WriteLine("");
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
