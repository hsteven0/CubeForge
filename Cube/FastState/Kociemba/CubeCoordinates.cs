using System;
using System.Collections.Generic;

namespace CubeForge.Cube.FastState.Kociemba
{
    public static class CubeCoordinates
    {
        public const int CornerOrientCount = 2187;
        public const int EdgeOrientCount = 2048;
        public const int UDSliceCount = 495;

        public const int CornerPermCount = 40320;
        public const int UDLayerPermCount = 40320;
        public const int ESlicePermCount = 24;

        public static readonly int SolvedCornerOrient = GetCornerOrientation(new FastCubeState());

        public static readonly int SolvedEdgeOrient = GetEdgeOrientation(new FastCubeState());

        public static readonly int SolvedUDSliceComb = GetUDSliceComb(new FastCubeState());

        public static readonly int SolvedCornerPerm = GetCornerPerm(new FastCubeState());

        public static readonly int SolvedUDLayerPerm = GetUDLayerPerm(new FastCubeState());

        public static readonly int SolvedESlicePerm = GetESlicePerm(new FastCubeState());

        public static Phase1Coordinate GetPhase1Coords(FastCubeState state)
        {
            return new Phase1Coordinate(GetCornerOrientation(state), GetEdgeOrientation(state), GetUDSliceComb(state));
        }

        public static Phase2Coordinate GetPhase2Coords(FastCubeState state)
        {
            return new Phase2Coordinate(GetCornerPerm(state), GetUDLayerPerm(state), GetESlicePerm(state));
        }

        public static int GetCornerOrientation(FastCubeState state)
        {
            int coordinate = 0;
            for (int i = 0; i < 7; i++)
                coordinate = coordinate * 3 + state.GetCornerOrientation((Corner)i);

            return coordinate;
        }

        public static int GetEdgeOrientation(FastCubeState state)
        {
            int coordinate = 0;

            for (int i = 0; i < 11; i++)
                coordinate = coordinate * 2 + state.GetEdgeOrientation((Edge)i);

            return coordinate;
        }

        public static int GetUDSliceComb(FastCubeState state)
        {
            bool[] selected = new bool[12];
            for (int position = 0; position < 12; position++)
            {
                Edge piece = state.GetEdgePiece((Edge)position);

                if (IsESliceEdge(piece))
                    selected[position] = true;
            }

            return CombinationIndexer.Rank(selected, 4);
        }

        public static int GetCornerPerm(FastCubeState state)
        {
            int[] permutation = new int[8];
            for (int i = 0; i < 8; i++)
                permutation[i] = (int)state.GetCornerPiece((Corner)i);

            return PermutationIndexer.Rank(permutation);
        }

        public static int GetUDLayerPerm(FastCubeState state)
        {
            List<int> values = new();

            for (int position = 0; position < 12; position++)
            {
                Edge piece = state.GetEdgePiece((Edge)position);

                if (!IsUDLayerEdge(piece))
                    continue;

                if (position >= 8)
                    return -1;

                values.Add(GetUDLayerIndex(piece));
            }

            if (values.Count != 8)
                return -1;
            return PermutationIndexer.Rank(values.ToArray());
        }

        public static int GetESlicePerm(FastCubeState state)
        {
            int[] eSlicePositions =
            {
                (int)Edge.FR,
                (int)Edge.FL,
                (int)Edge.BL,
                (int)Edge.BR
            };

            int[] permutation = new int[4];

            for (int i = 0; i < eSlicePositions.Length; i++)
            {
                Edge piece = state.GetEdgePiece((Edge)eSlicePositions[i]);
                if (!IsESliceEdge(piece))
                    return -1;

                permutation[i] = GetESliceIndex(piece);
            }

            return PermutationIndexer.Rank(permutation);
        }

        public static bool IsValidPhase2(FastCubeState state)
        {
            Phase2Coordinate coordinate = GetPhase2Coords(state);

            return coordinate.UDLayerPerm >= 0 && coordinate.ESlicePerm >= 0;
        }

        private static bool IsESliceEdge(Edge edge)
        {
            return edge is Edge.FR or Edge.FL or Edge.BL or Edge.BR;
        }

        private static bool IsUDLayerEdge(Edge edge)
        {
            return edge is Edge.UR or Edge.UF or Edge.UL or Edge.UB or
                           Edge.DR or Edge.DF or Edge.DL or Edge.DB;
        }

        private static int GetUDLayerIndex(Edge edge)
        {
            return edge switch
            {
                Edge.UR => 0,
                Edge.UF => 1,
                Edge.UL => 2,
                Edge.UB => 3,
                Edge.DR => 4,
                Edge.DF => 5,
                Edge.DL => 6,
                Edge.DB => 7,
                // test this
                _ => throw new ArgumentException($"Edge {edge} is not a U/D-layer edge.")
            };
        }

        private static int GetESliceIndex(Edge edge)
        {
            return edge switch
            {
                Edge.FR => 0,
                Edge.FL => 1,
                Edge.BL => 2,
                Edge.BR => 3,
                _ => throw new ArgumentException($"Edge {edge} is not an E-slice edge.")
            };
        }
    }
}
