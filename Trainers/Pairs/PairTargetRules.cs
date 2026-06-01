using CubeForge.Cube;
using CubeForge.Cube.FastState;
using CubeForge.Trainers.Shared;
using System;

namespace CubeForge.Trainers.Pairs
{
    public static class PairTargetRules
    {
        public static CaseTargetRules Create(PairSlot slot)
        {
            return new CaseTargetRules
            {
                FixOrientation = state => FixTargetOrientation(state, slot),
                IsValid = state => IsValidTarget(state, slot),
                InvalidMessage = caseName => $"Trusted target for {caseName} is not a valid pair-slot target."
            };
        }


        public static FastCubeState BuildTargetState(PairSlot slot, string setupMoves)
        {
            FastCubeState target = new FastCubeState();
            target.SetSolved();

            int yTurns = slot switch
            {
                PairSlot.FR => 0,
                PairSlot.FL => 3,
                PairSlot.BL => 2,
                PairSlot.BR => 1,
                _ => 0
            };

            if (yTurns > 0)
                target.ApplyMove(FaceMove.Y, yTurns);

            foreach (ParsedMove move in ScrambleParser.Parse(setupMoves))
                target.ApplyMove(move);

            if (yTurns > 0)
                target.ApplyMove(FaceMove.Y, 4 - yTurns);

            return target;
        }

        private static FastCubeState FixTargetOrientation(FastCubeState target, PairSlot slot)
        {
            for (int yTurns = 0; yTurns < 4; yTurns++)
            {
                FastCubeState candidate = target.Clone();

                if (yTurns > 0)
                    candidate.ApplyMove(FaceMove.Y, yTurns);

                if (IsValidTarget(candidate, slot))
                    return candidate;
            }

            return target;
        }

        private static bool IsValidTarget(FastCubeState state, PairSlot slot)
        {
            return IsCrossSolved(state) && OtherSlotsSolved(state, slot) && !IsSlotSolved(state, slot);
        }

        private static bool OtherSlotsSolved(FastCubeState state, PairSlot targetSlot)
        {
            foreach (PairSlot slot in Enum.GetValues(typeof(PairSlot)))
            {
                if (slot == targetSlot)
                    continue;

                if (!IsSlotSolved(state, slot))
                    return false;
            }

            return true;
        }

        private static bool IsCrossSolved(FastCubeState state)
        {
            return IsSolvedEdge(state, Edge.DR) &&
                   IsSolvedEdge(state, Edge.DF) &&
                   IsSolvedEdge(state, Edge.DL) &&
                   IsSolvedEdge(state, Edge.DB);
        }

        private static bool IsSlotSolved(FastCubeState state, PairSlot slot)
        {
            return slot switch
            {
                PairSlot.FR => IsSolvedCorner(state, Corner.DFR) && IsSolvedEdge(state, Edge.FR),
                PairSlot.FL => IsSolvedCorner(state, Corner.DLF) && IsSolvedEdge(state, Edge.FL),
                PairSlot.BL => IsSolvedCorner(state, Corner.DBL) && IsSolvedEdge(state, Edge.BL),
                PairSlot.BR => IsSolvedCorner(state, Corner.DRB) && IsSolvedEdge(state, Edge.BR),
                _ => false
            };
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
