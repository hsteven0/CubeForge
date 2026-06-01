using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CubeForge.Cube.FastState.Kociemba
{
    public class TwoPhaseSolver
    {
        private const int Solved = -1;
        private const int Aborted = -2;

        private int nodesSearched;
        private int maxNodes;
        private int timeoutMs;

        private bool abortedByNodeLimit;
        private bool abortedByTimeout;

        private readonly Stopwatch stopwatch = new();

        private FastCubeState originalStartState = null!;
        private FastCubeState currentPhase2StartState = null!;

        private bool randomizeMoveOrder;
        private Random random = new();

        public TwoPhaseSolution Solve(
            FastCubeState startState,
            int maxPhase1Depth = 12,
            int maxPhase2Depth = 18,
            int maxNodes = 2_000_000,
            int timeoutMs = 0,
            bool randomizeMoveOrder = false,
            int? randomSeed = null)
        {
            nodesSearched = 0;
            this.maxNodes = maxNodes <= 0 ? int.MaxValue : maxNodes;
            this.timeoutMs = timeoutMs;
            this.randomizeMoveOrder = randomizeMoveOrder;
            this.random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();

            abortedByNodeLimit = false;
            abortedByTimeout = false;

            originalStartState = startState.Clone();
            stopwatch.Restart();
            CoordinateMoveTables.Build();

            Phase1Coordinate startPhase1 = CubeCoordinates.GetPhase1Coords(startState);
            List<SearchMove> phase1Path = new();

            int initialBound = PruningTables.EstimatePhase1(startPhase1);
            for (int phase1Bound = initialBound; phase1Bound <= maxPhase1Depth; phase1Bound++)
            {
                if (ShouldAbort())
                    return GetFailure();

                phase1Path.Clear();

                int result = SearchPhase1(
                    startPhase1,
                    g: 0,
                    bound: phase1Bound,
                    path: phase1Path,
                    previousMove: null,
                    out TwoPhaseSolution? solution,
                    maxPhase2Depth: maxPhase2Depth);

                if (result == Solved && solution != null)
                {
                    solution.NodesSearched = nodesSearched;
                    return solution;
                }

                if (result == Aborted)
                    return GetFailure();
            }

            return new TwoPhaseSolution
            {
                Found = false,
                NodesSearched = nodesSearched,
                FailureReason = "No two phase solution found within the configured depth limits."
            };
        }

        private int SearchPhase1(
            Phase1Coordinate coordinate,
            int g,
            int bound,
            List<SearchMove> path,
            FaceMove? previousMove,
            out TwoPhaseSolution? solution,
            int maxPhase2Depth)
        {
            solution = null;

            if (VisitNode())
                return Aborted;

            int h = PruningTables.EstimatePhase1(coordinate);
            int f = g + h;

            if (f > bound)
                return f;

            if (IsPhase1Goal(coordinate))
            {
                FastCubeState phase2StartState = GetPhase2StartState(path);

                if (CubeCoordinates.IsValidPhase2(phase2StartState))
                {
                    TwoPhaseSolution phase2Solution = SolvePhase2(phase2StartState, path, maxPhase2Depth);

                    if (phase2Solution.Found)
                    {
                        solution = phase2Solution;
                        return Solved;
                    }

                    if (abortedByNodeLimit || abortedByTimeout)
                        return Aborted;
                }
            }

            int minNextBound = int.MaxValue;

            List<Phase1Node> candidates = GetPhase1Nodes(coordinate, previousMove, g, bound, ref minNextBound);
            SortPhase1Candidates(candidates);

            foreach (Phase1Node candidate in candidates)
            {
                path.Add(candidate.Move);

                int result = SearchPhase1(
                    candidate.Coordinate,
                    g + 1,
                    bound,
                    path,
                    candidate.Move.Move,
                    out solution,
                    maxPhase2Depth);

                if (result == Solved)
                    return Solved;

                if (result == Aborted)
                    return Aborted;

                if (result < minNextBound)
                    minNextBound = result;

                path.RemoveAt(path.Count - 1);
            }

            return minNextBound;
        }

        private TwoPhaseSolution SolvePhase2(FastCubeState phase2StartState, List<SearchMove> phase1Path, int maxPhase2Depth)
        {
            currentPhase2StartState = phase2StartState.Clone();
            if (phase2StartState.IsSolved())
            {
                return new TwoPhaseSolution
                {
                    Found = true,
                    Phase1Moves = new List<SearchMove>(phase1Path),
                    Phase2Moves = new List<SearchMove>()
                };
            }

            Phase2Coordinate startCoordinate = CubeCoordinates.GetPhase2Coords(phase2StartState);

            if (startCoordinate.UDLayerPerm < 0 || startCoordinate.ESlicePerm < 0)
            {
                return new TwoPhaseSolution { Found = false };
            }

            List<SearchMove> phase2Path = new();
            int initialBound = PruningTables.EstimatePhase2(startCoordinate);

            for (int bound = initialBound; bound <= maxPhase2Depth; bound++)
            {
                if (ShouldAbort())
                    return GetFailure();

                phase2Path.Clear();

                int result = SearchPhase2(
                    startCoordinate,
                    g: 0,
                    bound: bound,
                    path: phase2Path,
                    previousMove: phase1Path.Count > 0 ? phase1Path[^1].Move : null);

                if (result == Solved)
                {
                    return new TwoPhaseSolution
                    {
                        Found = true,
                        Phase1Moves = new List<SearchMove>(phase1Path),
                        Phase2Moves = new List<SearchMove>(phase2Path)
                    };
                }

                if (result == Aborted)
                    return GetFailure();
            }

            return new TwoPhaseSolution { Found = false };
        }

        private int SearchPhase2(Phase2Coordinate coordinate, int g, int bound, List<SearchMove> path, FaceMove? previousMove)
        {
            if (VisitNode())
                return Aborted;

            int h = PruningTables.EstimatePhase2(coordinate);
            int f = g + h;

            if (f > bound)
                return f;

            if (IsPhase2Goal(coordinate))
            {
                return VerifyPhase2Solve(path) ? Solved : int.MaxValue;
            }

            int minNextBound = int.MaxValue;

            List<Phase2Node> candidates = GetPhase2Nodes(coordinate, previousMove, g, bound, ref minNextBound);
            SortPhase2Candidates(candidates);

            foreach (Phase2Node candidate in candidates)
            {
                path.Add(candidate.Move);

                int result = SearchPhase2(candidate.Coordinate, g + 1, bound, path, candidate.Move.Move);

                if (result == Solved)
                    return Solved;

                if (result == Aborted)
                    return Aborted;

                if (result < minNextBound)
                    minNextBound = result;

                path.RemoveAt(path.Count - 1);
            }

            return minNextBound;
        }

        private List<Phase1Node> GetPhase1Nodes(Phase1Coordinate coordinate, FaceMove? previousMove, int g, int bound, ref int minNextBound)
        {
            List<Phase1Node> candidates = new(KociembaMoveSets.Phase1Moves.Count);

            for (int moveIndex = 0; moveIndex < KociembaMoveSets.Phase1Moves.Count; moveIndex++)
            {
                SearchMove move = KociembaMoveSets.Phase1Moves[moveIndex];

                if (ShouldSkipMove(move.Move, previousMove))
                    continue;

                Phase1Coordinate nextCoordinate = new(
                    CoordinateMoveTables.CornerOriMove[coordinate.CornerOrient, moveIndex],
                    CoordinateMoveTables.EdgeOriMove[coordinate.EdgeOrient, moveIndex],
                    CoordinateMoveTables.UDSliceMove[coordinate.UDSliceComb, moveIndex]
                );

                int h = PruningTables.EstimatePhase1(nextCoordinate);
                int f = g + 1 + h;

                if (f > bound)
                {
                    if (f < minNextBound)
                        minNextBound = f;

                    continue;
                }

                candidates.Add(new Phase1Node(
                    MoveIndex: moveIndex,
                    Move: move,
                    Coordinate: nextCoordinate,
                    Heuristic: h,
                    TieBreaker: randomizeMoveOrder ? random.Next() : moveIndex));
            }

            return candidates;
        }

        private List<Phase2Node> GetPhase2Nodes( Phase2Coordinate coordinate, FaceMove? previousMove, int g, int bound, ref int minNextBound)
        {
            List<Phase2Node> candidates = new(KociembaMoveSets.Phase2Moves.Count);

            for (int moveIndex = 0; moveIndex < KociembaMoveSets.Phase2Moves.Count; moveIndex++)
            {
                SearchMove move = KociembaMoveSets.Phase2Moves[moveIndex];

                if (ShouldSkipMove(move.Move, previousMove))
                    continue;

                Phase2Coordinate nextCoordinate = new(
                    CoordinateMoveTables.CornerPermMove[coordinate.CornerPerm, moveIndex],
                    CoordinateMoveTables.UDLayerPermMove[coordinate.UDLayerPerm, moveIndex],
                    CoordinateMoveTables.ESlicePermMove[coordinate.ESlicePerm, moveIndex]
                );

                int h = PruningTables.EstimatePhase2(nextCoordinate);
                int f = g + 1 + h;

                if (f > bound)
                {
                    if (f < minNextBound)
                        minNextBound = f;

                    continue;
                }

                candidates.Add(new Phase2Node(
                    MoveIndex: moveIndex,
                    Move: move,
                    Coordinate: nextCoordinate,
                    Heuristic: h,
                    TieBreaker: randomizeMoveOrder ? random.Next() : moveIndex));
            }

            return candidates;
        }

        private static void SortPhase1Candidates(List<Phase1Node> candidates)
        {
            candidates.Sort(static (a, b) =>
            {
                int compare = a.Heuristic.CompareTo(b.Heuristic);

                if (compare != 0)
                    return compare;

                return a.TieBreaker.CompareTo(b.TieBreaker);
            });
        }

        private static void SortPhase2Candidates(List<Phase2Node> candidates)
        {
            candidates.Sort(static (a, b) =>
            {
                int compare = a.Heuristic.CompareTo(b.Heuristic);

                if (compare != 0)
                    return compare;

                return a.TieBreaker.CompareTo(b.TieBreaker);
            });
        }

        private bool VerifyPhase2Solve(List<SearchMove> phase2Path)
        {
            FastCubeState check = currentPhase2StartState.Clone();

            foreach (SearchMove move in phase2Path)
                check.ApplyMove(move.Move, move.Turns);

            return check.IsSolved();
        }

        private FastCubeState GetPhase2StartState(List<SearchMove> phase1Path)
        {
            FastCubeState state = originalStartState.Clone();

            foreach (SearchMove move in phase1Path)
                state.ApplyMove(move.Move, move.Turns);

            return state;
        }

        private bool VisitNode()
        {
            nodesSearched++;
            return ShouldAbort();
        }

        private bool ShouldAbort()
        {
            if (nodesSearched >= maxNodes)
            {
                abortedByNodeLimit = true;
                return true;
            }

            if (timeoutMs > 0 &&
                stopwatch.ElapsedMilliseconds >= timeoutMs)
            {
                abortedByTimeout = true;
                return true;
            }

            return false;
        }

        private TwoPhaseSolution GetFailure()
        {
            return new TwoPhaseSolution
            {
                Found = false,
                NodesSearched = nodesSearched,
                HitNodeLimit = abortedByNodeLimit,
                TimedOut = abortedByTimeout,
                FailureReason = 
                    abortedByNodeLimit ? $"Node limit reached after {nodesSearched:N0} nodes."
                    : abortedByTimeout ? $"Timeout reached after {stopwatch.ElapsedMilliseconds:N0} ms."
                    : "Search aborted."
            };
        }

        private static bool IsPhase1Goal(Phase1Coordinate coordinate)
        {
            return coordinate.CornerOrient == CubeCoordinates.SolvedCornerOrient &&
                   coordinate.EdgeOrient == CubeCoordinates.SolvedEdgeOrient &&
                   coordinate.UDSliceComb == CubeCoordinates.SolvedUDSliceComb;
        }

        private static bool IsPhase2Goal(Phase2Coordinate coordinate)
        {
            return coordinate.CornerPerm == CubeCoordinates.SolvedCornerPerm &&
                   coordinate.UDLayerPerm == CubeCoordinates.SolvedUDLayerPerm &&
                   coordinate.ESlicePerm == CubeCoordinates.SolvedESlicePerm;
        }

        private static bool ShouldSkipMove(FaceMove current, FaceMove? previous)
        {
            if (previous == null)
                return false;

            FaceMove prev = previous.Value;
            if (current == prev)
                return true;

            if (AreOppositeFaces(current, prev))
                return GetFaceOrder(current) < GetFaceOrder(prev);

            return false;
        }

        private static bool AreOppositeFaces(FaceMove a, FaceMove b)
        {
            return
                (a == FaceMove.U && b == FaceMove.D) ||
                (a == FaceMove.D && b == FaceMove.U) ||
                (a == FaceMove.R && b == FaceMove.L) ||
                (a == FaceMove.L && b == FaceMove.R) ||
                (a == FaceMove.F && b == FaceMove.B) ||
                (a == FaceMove.B && b == FaceMove.F);
        }

        private static int GetFaceOrder(FaceMove move)
        {
            return move switch
            {
                FaceMove.U => 0,
                FaceMove.D => 1,
                FaceMove.R => 2,
                FaceMove.L => 3,
                FaceMove.F => 4,
                FaceMove.B => 5,
                _ => 99
            };
        }

        private readonly record struct Phase1Node(int MoveIndex, SearchMove Move, Phase1Coordinate Coordinate, int Heuristic, int TieBreaker);

        private readonly record struct Phase2Node(int MoveIndex, SearchMove Move, Phase2Coordinate Coordinate, int Heuristic, int TieBreaker);
    }
}
