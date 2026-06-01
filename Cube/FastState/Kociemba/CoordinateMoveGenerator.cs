using System;

namespace CubeForge.Cube.FastState.Kociemba
{
    public static class CoordinateMoveGenerator
    {
        public static FastCubeState FromCornerOrientation(int coordinate)
        {
            var state = new FastCubeState();

            int[] orientations = new int[8];
            int sum = 0;

            for (int i = 6; i >= 0; i--)
            {
                orientations[i] = coordinate % 3;
                coordinate /= 3;
                sum += orientations[i];
            }

            orientations[7] = (3 - (sum % 3)) % 3;

            for (int i = 0; i < 8; i++)
                state.SetCorner((Corner)i, (Corner)i, (byte)orientations[i]);

            return state;
        }

        public static FastCubeState FromEdgeOrientation(int coordinate)
        {
            var state = new FastCubeState();

            int[] orientations = new int[12];
            int sum = 0;

            for (int i = 10; i >= 0; i--)
            {
                orientations[i] = coordinate % 2;
                coordinate /= 2;
                sum += orientations[i];
            }

            orientations[11] = sum % 2;

            for (int i = 0; i < 12; i++)
                state.SetEdge((Edge)i, (Edge)i, (byte)orientations[i]);

            return state;
        }

        public static FastCubeState FromUDSliceComb(int coordinate)
        {
            var state = new FastCubeState();
            bool[] selected = CombinationIndexer.Unrank(12, 4, coordinate);

            Edge[] udEdges =
            {
                Edge.UR, Edge.UF, Edge.UL, Edge.UB,
                Edge.DR, Edge.DF, Edge.DL, Edge.DB
            };

            Edge[] eSliceEdges =
            {
                Edge.FR, Edge.FL, Edge.BL, Edge.BR
            };

            int udIndex = 0;
            int eIndex = 0;

            for (int position = 0; position < 12; position++)
            {
                Edge piece = selected[position] ? eSliceEdges[eIndex++] : udEdges[udIndex++];

                state.SetEdge((Edge)position, piece, 0);
            }

            return state;
        }

        public static FastCubeState FromCornerPerm(int coordinate)
        {
            var state = new FastCubeState();
            int[] permutation = PermutationIndexer.Unrank(8, coordinate);

            for (int position = 0; position < 8; position++)
                state.SetCorner((Corner)position, (Corner)permutation[position], 0);

            return state;
        }

        public static FastCubeState FromUDLayerPerm(int coordinate)
        {
            var state = new FastCubeState();
            int[] permutation = PermutationIndexer.Unrank(8, coordinate);

            Edge[] udEdges =
            {
                Edge.UR, Edge.UF, Edge.UL, Edge.UB,
                Edge.DR, Edge.DF, Edge.DL, Edge.DB
            };

            Edge[] eSliceEdges =
            {
                Edge.FR, Edge.FL, Edge.BL, Edge.BR
            };

            for (int position = 0; position < 8; position++)
                state.SetEdge((Edge)position, udEdges[permutation[position]], 0);

            for (int position = 8; position < 12; position++)
                state.SetEdge((Edge)position, eSliceEdges[position - 8], 0);

            return state;
        }

        public static FastCubeState FromESlicePerm(int coordinate)
        {
            var state = new FastCubeState();

            Edge[] udEdges =
            {
                Edge.UR, Edge.UF, Edge.UL, Edge.UB,
                Edge.DR, Edge.DF, Edge.DL, Edge.DB
            };

            Edge[] eSliceEdges =
            {
                Edge.FR, Edge.FL, Edge.BL, Edge.BR
            };

            for (int position = 0; position < 8; position++)
                state.SetEdge((Edge)position, udEdges[position], 0);

            int[] permutation = PermutationIndexer.Unrank(4, coordinate);

            for (int i = 0; i < 4; i++)
                state.SetEdge((Edge)(8 + i), eSliceEdges[permutation[i]], 0);

            return state;
        }
    }
}