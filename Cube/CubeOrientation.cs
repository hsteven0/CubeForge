using System;
using System.Windows.Media.Media3D;

namespace CubeForge.Cube
{
    public class CubeOrientation
    {
        public CubeColor BottomColor { get; }
        public CubeColor FrontColor { get; }

        public CubeOrientation(CubeColor bottomColor, CubeColor frontColor)
        {
            if (bottomColor == frontColor)
                throw new ArgumentException("Bottom color and front color cannot be the same.");

            if (CubeColorHelper.GetOpposite(bottomColor) == frontColor)
                throw new ArgumentException("Front color cannot be opposite of bottom color.");

            BottomColor = bottomColor;
            FrontColor = frontColor;
        }

        public Transform3D ToTransform()
        {
            Vector3D oldBottom = CubeColorHelper.GetSolvedDirection(BottomColor);
            Vector3D oldFront = CubeColorHelper.GetSolvedDirection(FrontColor);

            Vector3D oldUp = -oldBottom;
            Vector3D oldRight = Vector3D.CrossProduct(oldUp, oldFront);

            oldRight.Normalize();
            oldUp.Normalize();
            oldFront.Normalize();

            Matrix3D matrix = new Matrix3D(
                oldRight.X, oldUp.X, oldFront.X, 0,
                oldRight.Y, oldUp.Y, oldFront.Y, 0,
                oldRight.Z, oldUp.Z, oldFront.Z, 0,
                0, 0, 0, 1
            );

            return new MatrixTransform3D(matrix);
        }

        public ParsedMove ToBaseMove(ParsedMove move)
        {
            if (IsFaceOrWideMove(move.Move))
            {
                FaceMove mappedMove = MapMoveToBase(move.Move);
                return new ParsedMove(mappedMove, move.Turns, move.DoublePrime);
            }

            if (IsSliceMove(move.Move))
                return MapSliceToBase(move);

            if (IsRotationMove(move.Move))
                return MapRotationToBase(move);

            return move;
        }

        public FaceMove ToBaseMove(FaceMove move)
        {
            return ToBaseMove(new ParsedMove(move, 1)).Move;
        }

        private ParsedMove MapSliceToBase(ParsedMove move)
        {
            Face displayedReferenceFace = move.Move switch
            {
                FaceMove.M => Face.Left,
                FaceMove.E => Face.Down,
                FaceMove.S => Face.Front,
                _ => Face.Front
            };

            Face baseReferenceFace = MapFaceToBase(displayedReferenceFace);

            FaceMove baseSlice = baseReferenceFace switch
            {
                Face.Left or Face.Right => FaceMove.M,
                Face.Up or Face.Down => FaceMove.E,
                Face.Front or Face.Back => FaceMove.S,
                _ => move.Move
            };

            bool sameDirection = baseSlice switch
            {
                FaceMove.M => baseReferenceFace == Face.Left,
                FaceMove.E => baseReferenceFace == Face.Down,
                FaceMove.S => baseReferenceFace == Face.Front,
                _ => true
            };

            ParsedMove mapped = new ParsedMove(baseSlice, move.Turns, move.DoublePrime);
            return sameDirection ? mapped : InvertMove(mapped);
        }

        private ParsedMove MapRotationToBase(ParsedMove move)
        {
            Face displayedReferenceFace = move.Move switch
            {
                FaceMove.X => Face.Right,
                FaceMove.Y => Face.Up,
                FaceMove.Z => Face.Front,
                _ => Face.Front
            };

            Face baseReferenceFace = MapFaceToBase(displayedReferenceFace);

            FaceMove baseRotation = baseReferenceFace switch
            {
                Face.Left or Face.Right => FaceMove.X,
                Face.Up or Face.Down => FaceMove.Y,
                Face.Front or Face.Back => FaceMove.Z,
                _ => move.Move
            };

            bool sameDirection = baseRotation switch
            {
                FaceMove.X => baseReferenceFace == Face.Right,
                FaceMove.Y => baseReferenceFace == Face.Up,
                FaceMove.Z => baseReferenceFace == Face.Front,
                _ => true
            };

            ParsedMove mapped = new ParsedMove(baseRotation, move.Turns, move.DoublePrime);
            return sameDirection ? mapped : InvertMove(mapped);
        }

        private static ParsedMove InvertMove(ParsedMove move)
        {
            if (move.Turns == 1)
                return new ParsedMove(move.Move, 3, move.DoublePrime);

            if (move.Turns == 3)
                return new ParsedMove(move.Move, 1, move.DoublePrime);

            if (move.Turns == 2)
                return new ParsedMove(move.Move, 2, !move.DoublePrime);

            return move;
        }

        private static bool IsSliceMove(FaceMove move)
        {
            return move is FaceMove.M or FaceMove.E or FaceMove.S;
        }

        private static bool IsRotationMove(FaceMove move)
        {
            return move is FaceMove.X or FaceMove.Y or FaceMove.Z;
        }

        public FaceMove MapMoveToBase(FaceMove displayedMove)
        {
            if (!IsFaceOrWideMove(displayedMove))
                return displayedMove;

            Face displayedFace = GetFaceFromMove(displayedMove);
            Face baseFace = MapFaceToBase(displayedFace);
            bool wide = IsWideMove(displayedMove);

            return GetMoveFromFace(baseFace, wide);
        }

        public Face MapFaceToBase(Face displayedFace)
        {
            Vector3D desiredDisplayedDirection = GetFaceDirection(displayedFace);
            desiredDisplayedDirection.Normalize();

            Transform3D transform = ToTransform();

            foreach (CubeColor color in Enum.GetValues(typeof(CubeColor)))
            {
                Vector3D baseDirection = CubeColorHelper.GetSolvedDirection(color);
                Vector3D displayedDirection = transform.Transform(baseDirection);

                displayedDirection.Normalize();

                if (SameDirection(displayedDirection, desiredDisplayedDirection))
                    return GetFaceForColor(color);
            }

            throw new InvalidOperationException("Could not map displayed face to base face.");
        }

        private static bool IsFaceOrWideMove(FaceMove move)
        {
            return move is
                FaceMove.R or FaceMove.L or FaceMove.U or FaceMove.D or FaceMove.F or FaceMove.B or
                FaceMove.Rw or FaceMove.Lw or FaceMove.Uw or FaceMove.Dw or FaceMove.Fw or FaceMove.Bw;
        }

        private static bool IsWideMove(FaceMove move)
        {
            return move is FaceMove.Rw or FaceMove.Lw or FaceMove.Uw or FaceMove.Dw or FaceMove.Fw or FaceMove.Bw;
        }

        private static Face GetFaceFromMove(FaceMove move)
        {
            return move switch
            {
                FaceMove.R or FaceMove.Rw => Face.Right,
                FaceMove.L or FaceMove.Lw => Face.Left,
                FaceMove.U or FaceMove.Uw => Face.Up,
                FaceMove.D or FaceMove.Dw => Face.Down,
                FaceMove.F or FaceMove.Fw => Face.Front,
                FaceMove.B or FaceMove.Bw => Face.Back,
                _ => Face.Front
            };
        }

        private static FaceMove GetMoveFromFace(Face face, bool wide)
        {
            if (wide)
            {
                return face switch
                {
                    Face.Right => FaceMove.Rw,
                    Face.Left => FaceMove.Lw,
                    Face.Up => FaceMove.Uw,
                    Face.Down => FaceMove.Dw,
                    Face.Front => FaceMove.Fw,
                    Face.Back => FaceMove.Bw,
                    _ => FaceMove.Fw
                };
            }

            return face switch
            {
                Face.Right => FaceMove.R,
                Face.Left => FaceMove.L,
                Face.Up => FaceMove.U,
                Face.Down => FaceMove.D,
                Face.Front => FaceMove.F,
                Face.Back => FaceMove.B,
                _ => FaceMove.F
            };
        }

        private static Vector3D GetFaceDirection(Face face)
        {
            return face switch
            {
                Face.Right => new Vector3D(1, 0, 0),
                Face.Left => new Vector3D(-1, 0, 0),
                Face.Up => new Vector3D(0, 1, 0),
                Face.Down => new Vector3D(0, -1, 0),
                Face.Front => new Vector3D(0, 0, 1),
                Face.Back => new Vector3D(0, 0, -1),
                _ => new Vector3D(0, 0, 1)
            };
        }

        private static Face GetFaceForColor(CubeColor color)
        {
            return color switch
            {
                CubeColor.Blue => Face.Right,
                CubeColor.Green => Face.Left,
                CubeColor.White => Face.Up,
                CubeColor.Yellow => Face.Down,
                CubeColor.Red => Face.Front,
                CubeColor.Orange => Face.Back,
                _ => Face.Front
            };
        }

        private static bool SameDirection(Vector3D a, Vector3D b)
        {
            const double tolerance = 0.001;

            return Math.Abs(a.X - b.X) < tolerance &&
                   Math.Abs(a.Y - b.Y) < tolerance &&
                   Math.Abs(a.Z - b.Z) < tolerance;
        }
    }
}
