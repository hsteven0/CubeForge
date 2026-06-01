using CubeForge.Cube;
using CubeForge.Cube.FastState;
using System;
using System.Collections.Generic;

namespace CubeForge.Trainers.Shared
{
    public sealed class CaseTargetRules
    {
        public Func<FastCubeState, FastCubeState> FixOrientation { get; init; } = state => state;
        public Func<FastCubeState, bool> IsValid { get; init; } = state => true;
        public Func<string, string> InvalidMessage { get; init; } = caseName => $"Trusted target for {caseName} is not valid.";

        public static CaseTargetRules Oll { get; } = new CaseTargetRules
        {
            FixOrientation = FixOllOrientation,
            IsValid = IsValidOllTarget,
            InvalidMessage = caseName => $"Trusted target for {caseName} is not valid because F2L is not solved, or OLL already appears solved."
        };

        public static CaseTargetRules Pll { get; } = new CaseTargetRules
        {
            FixOrientation = FixPllOrientation,
            IsValid = IsValidPllTarget,
            InvalidMessage = caseName => $"Trusted target for {caseName} is not a valid PLL state."
        };

        private static FastCubeState FixOllOrientation(FastCubeState target)
        {
            for (int yTurns = 0; yTurns < 4; yTurns++)
            {
                FastCubeState candidate = target.Clone();

                if (yTurns > 0)
                    candidate.ApplyMove(FaceMove.Y, yTurns);

                if (IsF2LSolved(candidate))
                    return candidate;
            }

            return target;
        }

        private static bool IsValidOllTarget(FastCubeState state)
        {
            return IsF2LValid(state) && !IsLastLayerOrientationSolved(state);
        }

        private static FastCubeState FixPllOrientation(FastCubeState target)
        {
            for (int yTurns = 0; yTurns < 4; yTurns++)
            {
                FastCubeState candidate = target.Clone();

                if (yTurns > 0)
                    candidate.ApplyMove(FaceMove.Y, yTurns);

                if (IsValidPllTarget(candidate))
                    return candidate;
            }

            return target;
        }

        private static bool IsValidPllTarget(FastCubeState state)
        {
            return IsF2LSolved(state) &&
                   IsLastLayerOrientationSolved(state) &&
                   IsLastLayerPermutationOnly(state) &&
                   !state.IsSolved();
        }

        private static bool IsF2LSolved(FastCubeState state)
        {
            return IsSolvedCorner(state, Corner.DFR) &&
                   IsSolvedCorner(state, Corner.DLF) &&
                   IsSolvedCorner(state, Corner.DBL) &&
                   IsSolvedCorner(state, Corner.DRB) &&
                   IsSolvedEdge(state, Edge.DR) &&
                   IsSolvedEdge(state, Edge.DF) &&
                   IsSolvedEdge(state, Edge.DL) &&
                   IsSolvedEdge(state, Edge.DB) &&
                   IsSolvedEdge(state, Edge.FR) &&
                   IsSolvedEdge(state, Edge.FL) &&
                   IsSolvedEdge(state, Edge.BL) &&
                   IsSolvedEdge(state, Edge.BR);
        }

        private static bool IsF2LValid(FastCubeState state)
        {
            return AreDLayerCornersSolved(state) &&
                   AreDLayerEdgesSolved(state) &&
                   AreMiddleEdgesSolved(state);
        }

        private static bool AreDLayerCornersSolved(FastCubeState state)
        {
            HashSet<Corner> dCorners = new()
            {
                Corner.DFR,
                Corner.DLF,
                Corner.DBL,
                Corner.DRB
            };

            foreach (Corner position in dCorners)
            {
                Corner piece = state.GetCornerPiece(position);

                if (!dCorners.Contains(piece))
                    return false;

                if (state.GetCornerOrientation(position) != 0)
                    return false;
            }

            return true;
        }

        private static bool AreDLayerEdgesSolved(FastCubeState state)
        {
            HashSet<Edge> dEdges = new()
            {
                Edge.DR,
                Edge.DF,
                Edge.DL,
                Edge.DB
            };

            foreach (Edge position in dEdges)
            {
                Edge piece = state.GetEdgePiece(position);

                if (!dEdges.Contains(piece))
                    return false;

                if (state.GetEdgeOrientation(position) != 0)
                    return false;
            }

            return true;
        }

        private static bool AreMiddleEdgesSolved(FastCubeState state)
        {
            HashSet<Edge> middleEdges = new()
            {
                Edge.FR,
                Edge.FL,
                Edge.BL,
                Edge.BR
            };

            foreach (Edge position in middleEdges)
            {
                Edge piece = state.GetEdgePiece(position);

                if (!middleEdges.Contains(piece))
                    return false;

                if (state.GetEdgeOrientation(position) != 0)
                    return false;
            }

            return true;
        }

        private static bool IsLastLayerOrientationSolved(FastCubeState state)
        {
            return state.GetCornerOrientation(Corner.URF) == 0 &&
                   state.GetCornerOrientation(Corner.UFL) == 0 &&
                   state.GetCornerOrientation(Corner.ULB) == 0 &&
                   state.GetCornerOrientation(Corner.UBR) == 0 &&
                   state.GetEdgeOrientation(Edge.UR) == 0 &&
                   state.GetEdgeOrientation(Edge.UF) == 0 &&
                   state.GetEdgeOrientation(Edge.UL) == 0 &&
                   state.GetEdgeOrientation(Edge.UB) == 0;
        }

        private static bool IsLastLayerPermutationOnly(FastCubeState state)
        {
            return IsUCorner(state.GetCornerPiece(Corner.URF)) &&
                   IsUCorner(state.GetCornerPiece(Corner.UFL)) &&
                   IsUCorner(state.GetCornerPiece(Corner.ULB)) &&
                   IsUCorner(state.GetCornerPiece(Corner.UBR)) &&
                   IsUEdge(state.GetEdgePiece(Edge.UR)) &&
                   IsUEdge(state.GetEdgePiece(Edge.UF)) &&
                   IsUEdge(state.GetEdgePiece(Edge.UL)) &&
                   IsUEdge(state.GetEdgePiece(Edge.UB));
        }

        private static bool IsUCorner(Corner corner)
        {
            return corner == Corner.URF ||
                   corner == Corner.UFL ||
                   corner == Corner.ULB ||
                   corner == Corner.UBR;
        }

        private static bool IsUEdge(Edge edge)
        {
            return edge == Edge.UR ||
                   edge == Edge.UF ||
                   edge == Edge.UL ||
                   edge == Edge.UB;
        }

        private static bool IsSolvedCorner(FastCubeState state, Corner position)
        {
            return state.GetCornerPiece(position) == position && state.GetCornerOrientation(position) == 0;
        }

        private static bool IsSolvedEdge(FastCubeState state, Edge position)
        {
            return state.GetEdgePiece(position) == position && state.GetEdgeOrientation(position) == 0;
        }
    }
}
