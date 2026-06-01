using System;

namespace CubeForge.Cube.FastState
{
    public enum MoveSet
    {
        FaceTurnsOnly,
        FaceWideAndSliceTurns
    }

    public static class SearchMoveSets
    {
        public static readonly FaceMove[] FaceTurnsOnly =
        {
            FaceMove.R, FaceMove.L, FaceMove.U,
            FaceMove.D, FaceMove.F, FaceMove.B
        };

        public static readonly FaceMove[] FaceWideAndSliceTurns =
        {
            FaceMove.R, FaceMove.L, FaceMove.U,
            FaceMove.D, FaceMove.F, FaceMove.B,

            FaceMove.Rw, FaceMove.Lw,
            FaceMove.Uw, FaceMove.Fw,

            FaceMove.M
        };

        public static readonly int[] Turns =
        {
            1,
            2,
            3
        };

        public static FaceMove[] GetMoves(MoveSet kind)
        {
            return kind switch
            {
                MoveSet.FaceTurnsOnly => FaceTurnsOnly,
                MoveSet.FaceWideAndSliceTurns => FaceWideAndSliceTurns,
                _ => FaceTurnsOnly
            };
        }

        public static char GetAxis(FaceMove move)
        {
            return move switch
            {
                FaceMove.R or FaceMove.L or FaceMove.Rw or FaceMove.Lw or FaceMove.M => 'X',
                FaceMove.U or FaceMove.D or FaceMove.Uw or FaceMove.Dw or FaceMove.E => 'Y',
                FaceMove.F or FaceMove.B or FaceMove.Fw or FaceMove.Bw or FaceMove.S => 'Z',
                _ => '?'
            };
        }

        public static string GetFileKey(MoveSet moveSet)
        {
            return moveSet switch
            {
                MoveSet.FaceTurnsOnly => "face",
                MoveSet.FaceWideAndSliceTurns => "face+",
                _ => "face+"
            };
        }
    }
}
