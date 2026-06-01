using System.Collections.Generic;
using CubeForge.Cube.FastState;

namespace CubeForge.Cube.FastState.Kociemba
{
    public static class KociembaMoveSets
    {
        public static readonly IReadOnlyList<SearchMove> Phase1Moves = BuildPhase1Moves();
        public static readonly IReadOnlyList<SearchMove> Phase2Moves = BuildPhase2Moves();

        private static List<SearchMove> BuildPhase1Moves()
        {
            FaceMove[] faces =
            {
                FaceMove.U, FaceMove.D,
                FaceMove.R, FaceMove.L,
                FaceMove.F, FaceMove.B
            };
            return BuildMoves(faces, allowQuarterTurns: true);
        }

        private static List<SearchMove> BuildPhase2Moves()
        {
            var moves = new List<SearchMove>
            {
                new SearchMove(FaceMove.U, 1),
                new SearchMove(FaceMove.U, 2),
                new SearchMove(FaceMove.U, 3),

                new SearchMove(FaceMove.D, 1),
                new SearchMove(FaceMove.D, 2),
                new SearchMove(FaceMove.D, 3),

                new SearchMove(FaceMove.R, 2),
                new SearchMove(FaceMove.L, 2),
                new SearchMove(FaceMove.F, 2),
                new SearchMove(FaceMove.B, 2)
            };

            return moves;
        }

        private static List<SearchMove> BuildMoves(FaceMove[] faces, bool allowQuarterTurns)
        {
            var moves = new List<SearchMove>();

            foreach (FaceMove face in faces)
            {
                moves.Add(new SearchMove(face, 1));
                moves.Add(new SearchMove(face, 2));

                if (allowQuarterTurns)
                    moves.Add(new SearchMove(face, 3));
            }

            return moves;
        }
    }
}