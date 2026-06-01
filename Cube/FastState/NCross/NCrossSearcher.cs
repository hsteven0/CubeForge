using System;

namespace CubeForge.Cube.FastState
{
    public static class NCrossSearcher
    {
        private const int Found = -1;
        private static int nodeCount;

        public static MoveSet MoveSet { get; set; } = MoveSet.FaceTurnsOnly;

        private static FaceMove[] Moves => SearchMoveSets.GetMoves(MoveSet);
        private static int[] Turns => SearchMoveSets.Turns;

        // ida*
        public static NCrossSearchResult Solve(FastCubeState startState, NCrossTarget target, int maxDepth)
        {
            nodeCount = 0;
            List<SearchMove> path = new();
            int bound = EstimateDistance(startState, target);

            if (bound == 0)
            {
                return new NCrossSearchResult
                {
                    Found = true,
                    Target = target,
                    Solution = new List<SearchMove>(),
                    NodesSearched = nodeCount
                };
            }

            while (bound <= maxDepth)
            {
                FastCubeState workingState = startState.Clone();
                var visited = new Dictionary<ulong, int>();
                path.Clear();

                int result = Search(workingState, target, g: 0, bound: bound, path: path, previousMove: null, visited: visited);
                if (result == Found)
                {
                    return new NCrossSearchResult
                    {
                        Found = true,
                        Target = target,
                        Solution = new List<SearchMove>(path),
                        NodesSearched = nodeCount
                    };
                }

                if (result == int.MaxValue)
                    break;

                bound = result;
            }

            return new NCrossSearchResult
            {
                Found = false,
                Target = target,
                NodesSearched = nodeCount
            };
        }

        private static int Search(FastCubeState state, NCrossTarget target, int g, int bound, List<SearchMove> path, FaceMove? previousMove, Dictionary<ulong, int> visited)
        {
            nodeCount++;
            int h = EstimateDistance(state, target);
            int f = g + h;

            if (f > bound)
                return f;

            if (NCrossEvaluator.IsSolved(state, target))
                return Found;

            int depthRemaining = bound - g;

            if (depthRemaining == 1 && !CanSolveInOneMove(state, target, previousMove))
            {
                return int.MaxValue;
            }

            ulong key = state.GetHashKey();
            if (visited.TryGetValue(key, out int bestRemainingSeen) && bestRemainingSeen >= depthRemaining)
            {
                return int.MaxValue;
            }

            visited[key] = depthRemaining;
            int minNextBound = int.MaxValue;

            List<SearchMove> orderedMoves = GetOrderedMoves(state, target, previousMove);
            foreach (SearchMove move in orderedMoves)
            {
                state.ApplyMove(target.Orientation.ToBaseMove(new ParsedMove(move.Move, move.Turns)));

                path.Add(move);

                int result = Search(state, target, g + 1, bound, path, move.Move, visited);
                if (result == Found)
                    return Found;

                if (result < minNextBound)
                    minNextBound = result;

                path.RemoveAt(path.Count - 1);

                int undoTurns = 4 - move.Turns;
                state.ApplyMove(target.Orientation.ToBaseMove(new ParsedMove(move.Move, undoTurns)));
            }

            return minNextBound;
        }

        private static int EstimateDistance(FastCubeState state, NCrossTarget target)
        {
            int unsolvedCrossEdges = CrossEvaluator.CountUnsolvedCrossEdges(state, target);
            int unsolvedPairPieces = NCrossEvaluator.CountUnsolvedPairPieces(state, target);
            int unsolvedTotal = unsolvedCrossEdges + unsolvedPairPieces;

            int totalHeuristic = (unsolvedTotal + 2) / 3;
            int crossHeuristic = (unsolvedCrossEdges + 2) / 3;
            int pairHeuristic = (unsolvedPairPieces + 1) / 2;

            int pruningHeuristic = CrossPruningTable.Lookup(state, target, MoveSet);

            foreach (F2LSlot slot in target.Slots)
            {
                int pairTableHeuristic = PairPruningTable.Lookup(state, target, slot, MoveSet);
                int nCrossTableHeuristic = NCrossPruningTable.Lookup(state, target, slot, MoveSet);

                pruningHeuristic = Math.Max(pruningHeuristic, pairTableHeuristic);
                pruningHeuristic = Math.Max(pruningHeuristic, nCrossTableHeuristic);
            }

            return Math.Max(
                pruningHeuristic,
                Math.Max(totalHeuristic, Math.Max(crossHeuristic, pairHeuristic))
            );
        }

        private static List<SearchMove> GetOrderedMoves(FastCubeState state, NCrossTarget target, FaceMove? previousMove)
        {
            var candidates = new List<(SearchMove Move, int Score)>();
            int beforeScore = NCrossEvaluator.ScoreState(state, target);

            foreach (FaceMove move in Moves)
            {
                if (ShouldSkipMove(move, previousMove))
                    continue;

                foreach (int turns in Turns)
                {
                    state.ApplyMove(target.Orientation.ToBaseMove(new ParsedMove(move, turns)));

                    int afterScore = NCrossEvaluator.ScoreState(state, target);
                    int score = afterScore;

                    if (afterScore < beforeScore)
                        score -= 5;

                    if (afterScore > beforeScore)
                        score += 5;

                    int undoTurns = 4 - turns;
                    state.ApplyMove(target.Orientation.ToBaseMove(new ParsedMove(move, undoTurns)));

                    candidates.Add((new SearchMove(move, turns), score));
                }
            }

            return candidates.OrderBy(c => c.Score).Select(c => c.Move).ToList();
        }

        private static bool CanSolveInOneMove(FastCubeState state, NCrossTarget target, FaceMove? previousMove)
        {
            foreach (FaceMove move in Moves)
            {
                if (ShouldSkipMove(move, previousMove))
                    continue;

                foreach (int turns in Turns)
                {
                    state.ApplyMove(target.Orientation.ToBaseMove(new ParsedMove(move, turns)));
                    bool solved = NCrossEvaluator.IsSolved(state, target);

                    int undoTurns = 4 - turns;
                    state.ApplyMove(target.Orientation.ToBaseMove(new ParsedMove(move, undoTurns)));

                    if (solved)
                        return true;
                }
            }

            return false;
        }

        private static bool ShouldSkipMove(FaceMove current, FaceMove? previous)
        {
            if (previous == null)
                return false;

            if (current == previous.Value)
                return true;

            if (SearchMoveSets.GetAxis(current) == SearchMoveSets.GetAxis(previous.Value))
                return true;

            return false;
        }
    }
}
