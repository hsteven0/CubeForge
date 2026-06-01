using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeForge.Cube.FastState
{
    public enum F2LSlot
    {
        FrontRight,
        FrontLeft,
        BackLeft,
        BackRight
    }

    public static class PairEvaluator
    {
        public static bool IsPairSolved(FastCubeState cube, NCrossTarget target, F2LSlot slot)
        {
            target.GetSlotPieces(slot, out Corner corner, out Edge edge);
            return IsCornerSolved(cube, corner) && IsEdgeSolved(cube, edge);
        }

        public static int CountUnsolvedPairPieces(FastCubeState cube, NCrossTarget target, F2LSlot slot)
        {
            target.GetSlotPieces(slot, out Corner corner, out Edge edge);
            return CountUnsolvedPairPieces(cube, corner, edge);
        }

        private static int CountUnsolvedPairPieces(FastCubeState cube, Corner corner, Edge edge)
        {
            int count = 0;

            if (!IsCornerSolved(cube, corner)) count++;
            if (!IsEdgeSolved(cube, edge)) count++;

            return count;
        }

        private static bool IsCornerSolved(FastCubeState cube, Corner position)
        {
            return cube.GetCornerPiece(position) == position && cube.GetCornerOrientation(position) == 0;
        }

        private static bool IsEdgeSolved(FastCubeState cube, Edge position)
        {
            return cube.GetEdgePiece(position) == position && cube.GetEdgeOrientation(position) == 0;
        }
    }
}
