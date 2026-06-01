using System;
using System.Text;

namespace CubeForge.Cube.FastState
{
    public class FastCubeState
    {
        private readonly byte[] cornerPermutation = new byte[8];
        private readonly byte[] cornerOrientation = new byte[8];

        private readonly byte[] edgePermutation = new byte[12];
        private readonly byte[] edgeOrientation = new byte[12];

        public FastCubeState()
        {
            SetSolved();
        }

        public void SetSolved()
        {
            for (byte i = 0; i < 8; i++)
            {
                cornerPermutation[i] = i;
                cornerOrientation[i] = 0;
            }

            for (byte i = 0; i < 12; i++)
            {
                edgePermutation[i] = i;
                edgeOrientation[i] = 0;
            }
        }

        public FastCubeState Clone()
        {
            var copy = new FastCubeState();

            Array.Copy(cornerPermutation, copy.cornerPermutation, cornerPermutation.Length);
            Array.Copy(cornerOrientation, copy.cornerOrientation, cornerOrientation.Length);

            Array.Copy(edgePermutation, copy.edgePermutation, edgePermutation.Length);
            Array.Copy(edgeOrientation, copy.edgeOrientation, edgeOrientation.Length);

            return copy;
        }

        public byte GetCornerPermutation(Corner position)
        {
            return cornerPermutation[(int)position];
        }

        public byte GetCornerOrientation(Corner position)
        {
            return cornerOrientation[(int)position];
        }

        public void SetCorner(Corner position, Corner piece, byte orientation)
        {
            if (orientation > 2)
                throw new ArgumentOutOfRangeException(nameof(orientation), "Corner orientation must be 0, 1, or 2.");

            cornerPermutation[(int)position] = (byte)piece;
            cornerOrientation[(int)position] = orientation;
        }

        public byte GetEdgePermutation(Edge position)
        {
            return edgePermutation[(int)position];
        }

        public byte GetEdgeOrientation(Edge position)
        {
            return edgeOrientation[(int)position];
        }

        public void SetEdge(Edge position, Edge piece, byte orientation)
        {
            if (orientation > 1)
                throw new ArgumentOutOfRangeException(nameof(orientation), "Edge orientation must be 0 or 1.");

            edgePermutation[(int)position] = (byte)piece;
            edgeOrientation[(int)position] = orientation;
        }

        public Corner GetCornerPiece(Corner position)
        {
            return (Corner)cornerPermutation[(int)position];
        }

        public Edge GetEdgePiece(Edge position)
        {
            return (Edge)edgePermutation[(int)position];
        }

        public void CopyFrom(FastCubeState other)
        {
            Array.Copy(other.cornerPermutation, cornerPermutation, cornerPermutation.Length);
            Array.Copy(other.cornerOrientation, cornerOrientation, cornerOrientation.Length);

            Array.Copy(other.edgePermutation, edgePermutation, edgePermutation.Length);
            Array.Copy(other.edgeOrientation, edgeOrientation, edgeOrientation.Length);
        }

        public bool IsSolved()
        {
            for (byte i = 0; i < 8; i++)
            {
                if (cornerPermutation[i] != i || cornerOrientation[i] != 0)
                    return false;
            }

            for (byte i = 0; i < 12; i++)
            {
                if (edgePermutation[i] != i || edgeOrientation[i] != 0)
                    return false;
            }

            return true;
        }

        private static void Cycle4<T>(T[] array, int a, int b, int c, int d)
        {
            T temp = array[d];
            array[d] = array[c];
            array[c] = array[b];
            array[b] = array[a];
            array[a] = temp;
        }

        private static void Cycle4Corners(
            byte[] perm,
            byte[] ori,
            int a,
            int b,
            int c,
            int d,
            int deltaA,
            int deltaB,
            int deltaC,
            int deltaD)
        {
            byte oldPermA = perm[a];
            byte oldPermB = perm[b];
            byte oldPermC = perm[c];
            byte oldPermD = perm[d];

            byte oldOriA = ori[a];
            byte oldOriB = ori[b];
            byte oldOriC = ori[c];
            byte oldOriD = ori[d];

            perm[a] = oldPermD;
            perm[b] = oldPermA;
            perm[c] = oldPermB;
            perm[d] = oldPermC;

            ori[a] = (byte)((oldOriD + deltaA) % 3);
            ori[b] = (byte)((oldOriA + deltaB) % 3);
            ori[c] = (byte)((oldOriB + deltaC) % 3);
            ori[d] = (byte)((oldOriC + deltaD) % 3);
        }

        private static void Cycle4Edges(
            byte[] perm,
            byte[] ori,
            int a,
            int b,
            int c,
            int d,
            bool flip)
        {
            byte oldPermA = perm[a];
            byte oldPermB = perm[b];
            byte oldPermC = perm[c];
            byte oldPermD = perm[d];

            byte oldOriA = ori[a];
            byte oldOriB = ori[b];
            byte oldOriC = ori[c];
            byte oldOriD = ori[d];

            perm[a] = oldPermD;
            perm[b] = oldPermA;
            perm[c] = oldPermB;
            perm[d] = oldPermC;

            ori[a] = (byte)(oldOriD ^ (flip ? 1 : 0));
            ori[b] = (byte)(oldOriA ^ (flip ? 1 : 0));
            ori[c] = (byte)(oldOriB ^ (flip ? 1 : 0));
            ori[d] = (byte)(oldOriC ^ (flip ? 1 : 0));
        }

        public void ApplyMove(ParsedMove move)
        {
            ApplyMove(move.Move, move.Turns, move.DoublePrime);
        }

        public void ApplyMove(FaceMove move, int turns, bool doublePrime = false)
        {
            turns = (turns % 4 + 4) % 4;

            if (turns == 0)
                return;

            if (turns == 1)
            {
                ApplyMove(move, prime: false);
            }
            else if (turns == 2)
            {
                ApplyMove(move, prime: doublePrime);
                ApplyMove(move, prime: doublePrime);
            }
            else if (turns == 3)
            {
                ApplyMove(move, prime: true);
            }
        }

        private void ApplyMove(FaceMove move, bool prime)
        {
            switch (move)
            {
                case FaceMove.R:
                    MoveR(prime);
                    break;
                case FaceMove.L:
                    MoveL(prime);
                    break;
                case FaceMove.U:
                    MoveU(prime);
                    break;
                case FaceMove.D:
                    MoveD(prime);
                    break;
                case FaceMove.F:
                    MoveF(prime);
                    break;
                case FaceMove.B:
                    MoveB(prime);
                    break;
                
                // Slice moves
                case FaceMove.M:
                    MoveM(prime);
                    break;
                case FaceMove.E:
                    MoveE(prime);
                    break;
                case FaceMove.S:
                    MoveS(prime);
                    break;

                // Wide moves
                case FaceMove.Rw:
                    MoveR(prime);
                    MoveM(!prime);
                    break;
                case FaceMove.Lw:
                    MoveL(prime);
                    MoveM(prime);
                    break;
                case FaceMove.Uw:
                    MoveU(prime);
                    MoveE(!prime);
                    break;
                case FaceMove.Dw:
                    MoveD(prime);
                    MoveE(prime);
                    break;
                case FaceMove.Fw:
                    MoveF(prime);
                    MoveS(prime);
                    break;
                case FaceMove.Bw:
                    MoveB(prime);
                    MoveS(!prime);
                    break;
                case FaceMove.X:
                    MoveR(prime);
                    MoveM(!prime);
                    MoveL(!prime);
                    break;
                case FaceMove.Y:
                    MoveU(prime);
                    MoveE(!prime);
                    MoveD(!prime);
                    break;
                case FaceMove.Z:
                    MoveF(prime);
                    MoveS(prime);
                    MoveB(!prime);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported move: {move}");
            }
        }

        public static FastCubeState FromAlgorithm(string algorithm)
        {
            FastCubeState state = new FastCubeState();
            foreach (ParsedMove move in ScrambleParser.Parse(algorithm))
                state.ApplyMove(move);
            return state;
        }

        private static void Cycle4Inverse<T>(T[] array, int a, int b, int c, int d)
        {
            T temp = array[a];
            array[a] = array[b];
            array[b] = array[c];
            array[c] = array[d];
            array[d] = temp;
        }

        private static byte SubtractCornerOrientation(byte value, int delta)
        {
            return (byte)((value - delta + 3) % 3);
        }

        private static void Cycle4CornersInverse(byte[] perm, byte[] ori, int a, int b, int c, int d, int deltaA, int deltaB, int deltaC, int deltaD)
        {
            byte oldPermA = perm[a];
            byte oldPermB = perm[b];
            byte oldPermC = perm[c];
            byte oldPermD = perm[d];

            byte oldOriA = ori[a];
            byte oldOriB = ori[b];
            byte oldOriC = ori[c];
            byte oldOriD = ori[d];

            perm[a] = oldPermB;
            perm[b] = oldPermC;
            perm[c] = oldPermD;
            perm[d] = oldPermA;

            ori[a] = SubtractCornerOrientation(oldOriB, deltaB);
            ori[b] = SubtractCornerOrientation(oldOriC, deltaC);
            ori[c] = SubtractCornerOrientation(oldOriD, deltaD);
            ori[d] = SubtractCornerOrientation(oldOriA, deltaA);
        }

        private static void Cycle4EdgesInverse(byte[] perm, byte[] ori, int a, int b, int c, int d, bool flip)
        {
            byte oldPermA = perm[a];
            byte oldPermB = perm[b];
            byte oldPermC = perm[c];
            byte oldPermD = perm[d];

            byte oldOriA = ori[a];
            byte oldOriB = ori[b];
            byte oldOriC = ori[c];
            byte oldOriD = ori[d];

            perm[a] = oldPermB;
            perm[b] = oldPermC;
            perm[c] = oldPermD;
            perm[d] = oldPermA;

            ori[a] = (byte)(oldOriB ^ (flip ? 1 : 0));
            ori[b] = (byte)(oldOriC ^ (flip ? 1 : 0));
            ori[c] = (byte)(oldOriD ^ (flip ? 1 : 0));
            ori[d] = (byte)(oldOriA ^ (flip ? 1 : 0));
        }

        private void MoveU(bool prime)
        {
            if (!prime)
            {
                Cycle4(cornerPermutation, (int)Corner.URF, (int)Corner.UFL, (int)Corner.ULB, (int)Corner.UBR);
                Cycle4(cornerOrientation, (int)Corner.URF, (int)Corner.UFL, (int)Corner.ULB, (int)Corner.UBR);
                Cycle4(edgePermutation, (int)Edge.UR, (int)Edge.UF, (int)Edge.UL, (int)Edge.UB);
                Cycle4(edgeOrientation, (int)Edge.UR, (int)Edge.UF, (int)Edge.UL, (int)Edge.UB);
            }
            else
            {
                Cycle4Inverse(cornerPermutation, (int)Corner.URF, (int)Corner.UFL, (int)Corner.ULB, (int)Corner.UBR);
                Cycle4Inverse(cornerOrientation, (int)Corner.URF, (int)Corner.UFL, (int)Corner.ULB, (int)Corner.UBR);
                Cycle4Inverse(edgePermutation, (int)Edge.UR, (int)Edge.UF, (int)Edge.UL, (int)Edge.UB);
                Cycle4Inverse(edgeOrientation, (int)Edge.UR, (int)Edge.UF, (int)Edge.UL, (int)Edge.UB);
            }
        }

        private void MoveD(bool prime)
        {
            if (!prime)
            {
                Cycle4(cornerPermutation, (int)Corner.DFR, (int)Corner.DRB, (int)Corner.DBL, (int)Corner.DLF);
                Cycle4(cornerOrientation, (int)Corner.DFR, (int)Corner.DRB, (int)Corner.DBL, (int)Corner.DLF);
                Cycle4(edgePermutation, (int)Edge.DR, (int)Edge.DB, (int)Edge.DL, (int)Edge.DF);
                Cycle4(edgeOrientation, (int)Edge.DR, (int)Edge.DB, (int)Edge.DL, (int)Edge.DF);
            }
            else
            {
                Cycle4Inverse(cornerPermutation, (int)Corner.DFR, (int)Corner.DRB, (int)Corner.DBL, (int)Corner.DLF);
                Cycle4Inverse(cornerOrientation, (int)Corner.DFR, (int)Corner.DRB, (int)Corner.DBL, (int)Corner.DLF);
                Cycle4Inverse(edgePermutation, (int)Edge.DR, (int)Edge.DB, (int)Edge.DL, (int)Edge.DF);
                Cycle4Inverse(edgeOrientation, (int)Edge.DR, (int)Edge.DB, (int)Edge.DL, (int)Edge.DF);
            }
        }

        private void MoveR(bool prime)
        {
            if (!prime)
            {
                Cycle4Corners(cornerPermutation, cornerOrientation, (int)Corner.URF, (int)Corner.UBR, (int)Corner.DRB, (int)Corner.DFR, 2, 1, 2, 1);
                Cycle4Edges(edgePermutation, edgeOrientation, (int)Edge.UR, (int)Edge.BR, (int)Edge.DR, (int)Edge.FR, flip: false);
            }
            else
            {
                Cycle4CornersInverse(cornerPermutation, cornerOrientation, (int)Corner.URF, (int)Corner.UBR, (int)Corner.DRB, (int)Corner.DFR, 2, 1, 2, 1);
                Cycle4EdgesInverse(edgePermutation, edgeOrientation, (int)Edge.UR, (int)Edge.BR, (int)Edge.DR, (int)Edge.FR, flip: false);
            }
        }

        private void MoveL(bool prime)
        {
            if (!prime)
            {
                Cycle4Corners(cornerPermutation, cornerOrientation, (int)Corner.UFL, (int)Corner.DLF, (int)Corner.DBL, (int)Corner.ULB, 1, 2, 1, 2);
                Cycle4Edges(edgePermutation, edgeOrientation, (int)Edge.UL, (int)Edge.FL, (int)Edge.DL, (int)Edge.BL, flip: false);
            }
            else
            {
                Cycle4CornersInverse(cornerPermutation, cornerOrientation, (int)Corner.UFL, (int)Corner.DLF, (int)Corner.DBL, (int)Corner.ULB, 1, 2, 1, 2);
                Cycle4EdgesInverse(edgePermutation, edgeOrientation, (int)Edge.UL, (int)Edge.FL, (int)Edge.DL, (int)Edge.BL, flip: false);
            }
        }

        private void MoveF(bool prime)
        {
            if (!prime)
            {
                Cycle4Corners(cornerPermutation, cornerOrientation, (int)Corner.URF, (int)Corner.DFR, (int)Corner.DLF, (int)Corner.UFL, 1, 2, 1, 2);
                Cycle4Edges(edgePermutation, edgeOrientation, (int)Edge.UF, (int)Edge.FR, (int)Edge.DF, (int)Edge.FL, flip: true);
            }
            else
            {
                Cycle4CornersInverse(cornerPermutation, cornerOrientation, (int)Corner.URF, (int)Corner.DFR, (int)Corner.DLF, (int)Corner.UFL, 1, 2, 1, 2);
                Cycle4EdgesInverse(edgePermutation, edgeOrientation, (int)Edge.UF, (int)Edge.FR, (int)Edge.DF, (int)Edge.FL, flip: true);
            }
        }

        private void MoveB(bool prime)
        {
            if (!prime)
            {
                Cycle4Corners(cornerPermutation, cornerOrientation, (int)Corner.UBR, (int)Corner.ULB, (int)Corner.DBL, (int)Corner.DRB, 2, 1, 2, 1);
                Cycle4Edges(edgePermutation, edgeOrientation, (int)Edge.UB, (int)Edge.BL, (int)Edge.DB, (int)Edge.BR, flip: true);
            }
            else
            {
                Cycle4CornersInverse(cornerPermutation, cornerOrientation, (int)Corner.UBR, (int)Corner.ULB, (int)Corner.DBL, (int)Corner.DRB, 2, 1, 2, 1);
                Cycle4EdgesInverse(edgePermutation, edgeOrientation, (int)Edge.UB, (int)Edge.BL, (int)Edge.DB, (int)Edge.BR, flip: true);
            }
        }

        // M turns like L on the middle slice.
        private void MoveM(bool prime)
        {
            if (!prime)
                Cycle4Edges(edgePermutation, edgeOrientation, (int)Edge.UF, (int)Edge.DF, (int)Edge.DB, (int)Edge.UB, flip: true);
            else
                Cycle4EdgesInverse(edgePermutation, edgeOrientation, (int)Edge.UF, (int)Edge.DF, (int)Edge.DB, (int)Edge.UB, flip: true);
        }

        // E turns like D on the middle slice.
        private void MoveE(bool prime)
        {
            if (!prime)
                Cycle4Edges(edgePermutation, edgeOrientation, (int)Edge.FR, (int)Edge.BR, (int)Edge.BL, (int)Edge.FL, flip: true);
            else
                Cycle4EdgesInverse(edgePermutation, edgeOrientation, (int)Edge.FR, (int)Edge.BR, (int)Edge.BL, (int)Edge.FL, flip: true);
        }

        // S turns like F on the middle slice.
        private void MoveS(bool prime)
        {
            if (!prime)
                Cycle4Edges(edgePermutation, edgeOrientation, (int)Edge.UR, (int)Edge.DR, (int)Edge.DL, (int)Edge.UL, flip: true);
            else
                Cycle4EdgesInverse(edgePermutation, edgeOrientation, (int)Edge.UR, (int)Edge.DR, (int)Edge.DL, (int)Edge.UL, flip: true);
        }

        public ulong GetHashKey()
        {
            ulong hash = 14695981039346656037UL; // FNV offset basis

            void AddByte(byte value)
            {
                hash ^= value;
                hash *= 1099511628211UL; // FNV prime
            }

            for (int i = 0; i < 8; i++)
            {
                AddByte(cornerPermutation[i]);
                AddByte(cornerOrientation[i]);
            }

            for (int i = 0; i < 12; i++)
            {
                AddByte(edgePermutation[i]);
                AddByte(edgeOrientation[i]);
            }

            return hash;
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Corners:");
            for (int i = 0; i < 8; i++)
            {
                sb.AppendLine($"{(Corner)i}: piece={(Corner)cornerPermutation[i]}, ori={cornerOrientation[i]}");
            }

            sb.AppendLine();

            sb.AppendLine("Edges:");
            for (int i = 0; i < 12; i++)
            {
                sb.AppendLine($"{(Edge)i}: piece={(Edge)edgePermutation[i]}, ori={edgeOrientation[i]}");
            }

            return sb.ToString();
        }

        public string ToPieceDebugString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Corners:");
            for (int i = 0; i < 8; i++)
            {
                sb.AppendLine($"{(Corner)i}: piece={GetCornerPiece((Corner)i)}, ori={GetCornerOrientation((Corner)i)}");
            }

            sb.AppendLine();
            sb.AppendLine("Edges:");
            for (int i = 0; i < 12; i++)
            {
                sb.AppendLine($"{(Edge)i}: piece={GetEdgePiece((Edge)i)}, ori={GetEdgeOrientation((Edge)i)}");
            }

            return sb.ToString();
        }

        public Corner FindCornerPosition(Corner targetPiece)
        {
            for (int i = 0; i < 8; i++)
            {
                if ((Corner)cornerPermutation[i] == targetPiece)
                    return (Corner)i;
            }

            throw new InvalidOperationException($"Corner piece {targetPiece} not found.");
        }

        public Edge FindEdgePosition(Edge targetPiece)
        {
            for (int i = 0; i < 12; i++)
            {
                if ((Edge)edgePermutation[i] == targetPiece)
                    return (Edge)i;
            }

            throw new InvalidOperationException($"Edge piece {targetPiece} not found.");
        }
    }
}