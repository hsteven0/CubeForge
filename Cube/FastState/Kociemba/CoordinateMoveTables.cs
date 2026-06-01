using System;

namespace CubeForge.Cube.FastState.Kociemba
{
    public static class CoordinateMoveTables
    {
        public static int[,] CornerOriMove { get; private set; } = null!;
        public static int[,] EdgeOriMove { get; private set; } = null!;
        public static int[,] UDSliceMove { get; private set; } = null!;

        public static int[,] CornerPermMove { get; private set; } = null!;
        public static int[,] UDLayerPermMove { get; private set; } = null!;
        public static int[,] ESlicePermMove { get; private set; } = null!;

        public static bool IsBuilt { get; private set; }

        public static void Build()
        {
            if (IsBuilt)
                return;

            BuildPhase1Tables();
            BuildPhase2Tables();

            IsBuilt = true;
        }

        private static void BuildPhase1Tables()
        {
            CornerOriMove = BuildTable(
                CubeCoordinates.CornerOrientCount,
                KociembaMoveSets.Phase1Moves.Count,
                CoordinateMoveGenerator.FromCornerOrientation,
                CubeCoordinates.GetCornerOrientation,
                usePhase1Moves: true);

            EdgeOriMove = BuildTable(
                CubeCoordinates.EdgeOrientCount,
                KociembaMoveSets.Phase1Moves.Count,
                CoordinateMoveGenerator.FromEdgeOrientation,
                CubeCoordinates.GetEdgeOrientation,
                usePhase1Moves: true);

            UDSliceMove = BuildTable(
                CubeCoordinates.UDSliceCount,
                KociembaMoveSets.Phase1Moves.Count,
                CoordinateMoveGenerator.FromUDSliceComb,
                CubeCoordinates.GetUDSliceComb,
                usePhase1Moves: true);
        }

        private static void BuildPhase2Tables()
        {
            CornerPermMove = BuildTable(
                CubeCoordinates.CornerPermCount,
                KociembaMoveSets.Phase2Moves.Count,
                CoordinateMoveGenerator.FromCornerPerm,
                CubeCoordinates.GetCornerPerm,
                usePhase1Moves: false);

            UDLayerPermMove = BuildTable(
                CubeCoordinates.UDLayerPermCount,
                KociembaMoveSets.Phase2Moves.Count,
                CoordinateMoveGenerator.FromUDLayerPerm,
                CubeCoordinates.GetUDLayerPerm,
                usePhase1Moves: false);

            ESlicePermMove = BuildTable(
                CubeCoordinates.ESlicePermCount,
                KociembaMoveSets.Phase2Moves.Count,
                CoordinateMoveGenerator.FromESlicePerm,
                CubeCoordinates.GetESlicePerm,
                usePhase1Moves: false);
        }

        private static int[,] BuildTable(int coordCount, int moveCount, Func<int, FastCubeState> stateFromCoord, Func<FastCubeState, int> coordFromState, bool usePhase1Moves)
        {
            int[,] table = new int[coordCount, moveCount];
            for (int coord = 0; coord < coordCount; coord++)
            {
                for (int moveIndex = 0; moveIndex < moveCount; moveIndex++)
                {
                    SearchMove move = usePhase1Moves ? KociembaMoveSets.Phase1Moves[moveIndex] : KociembaMoveSets.Phase2Moves[moveIndex];

                    FastCubeState state = stateFromCoord(coord);
                    state.ApplyMove(move.Move, move.Turns);

                    table[coord, moveIndex] = coordFromState(state);
                }
            }
            return table;
        }
    }
}