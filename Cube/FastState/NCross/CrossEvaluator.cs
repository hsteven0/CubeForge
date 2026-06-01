namespace CubeForge.Cube.FastState
{
    public static class CrossEvaluator
    {
        public static bool IsCrossSolved(FastCubeState cube, NCrossTarget target)
        {
            foreach (Edge edge in target.GetCrossEdges())
            {
                if (!IsCrossEdgeSolved(cube, edge))
                    return false;
            }
            return true;
        }

        public static int CountUnsolvedCrossEdges(FastCubeState cube, NCrossTarget target)
        {
            int count = 0;
            foreach (Edge edge in target.GetCrossEdges())
            {
                if (!IsCrossEdgeSolved(cube, edge))
                    count++;
            }

            return count;
        }

        private static bool IsCrossEdgeSolved(FastCubeState cube, Edge position)
        {
            return cube.GetEdgePiece(position) == position && cube.GetEdgeOrientation(position) == 0;
        }
    }
}
