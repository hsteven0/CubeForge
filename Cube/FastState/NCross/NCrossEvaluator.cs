namespace CubeForge.Cube.FastState
{
    public static class NCrossEvaluator
    {
        public static bool IsSolved(FastCubeState cube, NCrossTarget target)
        {
            if (!CrossEvaluator.IsCrossSolved(cube, target))
                return false;

            foreach (F2LSlot slot in target.Slots)
            {
                if (!PairEvaluator.IsPairSolved(cube, target, slot))
                    return false;
            }

            return true;
        }

        public static int CountUnsolvedPairPieces(FastCubeState cube, NCrossTarget target)
        {
            int count = 0;

            foreach (F2LSlot slot in target.Slots)
            {
                count += PairEvaluator.CountUnsolvedPairPieces(cube, target, slot);
            }

            return count;
        }

        public static int ScoreState(FastCubeState cube, NCrossTarget target)
        {
            int score = 0;
            score += CrossEvaluator.CountUnsolvedCrossEdges(cube, target) * 10;

            foreach (F2LSlot slot in target.Slots)
            {
                score += PairEvaluator.CountUnsolvedPairPieces(cube, target, slot) * 12;
            }

            return score;
        }
    }
}
