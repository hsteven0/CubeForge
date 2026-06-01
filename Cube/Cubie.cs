using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace CubeForge.Cube
{
    public class Cubie
    {
        // Position in cube
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        // Sticker colors
        public Dictionary<Face, Color?> Stickers { get; set; }

        // 3D model
        public ModelVisual3D Visual { get; set; }

        public Cubie(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;

            Stickers = new Dictionary<Face, Color?>()
            {
                { Face.Up, null },
                { Face.Down, null },
                { Face.Left, null },
                { Face.Right, null },
                { Face.Front, null },
                { Face.Back, null }
            };
        }

        public void SetSticker(Face face, Color color)
        {
            Stickers[face] = color;
        }

        public void ClearStickers()
        {
            foreach (var key in Stickers.Keys) 
                Stickers[key] = null;
        }

        // Rotate cubie stickers around +X axis clockwise
        // (clockwise when looking directly at the right face)
		// Left and Right do not change for rotation around X
        public void RotateAroundXClockwise()
        {
            Color? oldUp = Stickers[Face.Up];
            Color? oldFront = Stickers[Face.Front];
            Color? oldDown = Stickers[Face.Down];
            Color? oldBack = Stickers[Face.Back];

            Stickers[Face.Up] = oldFront;
            Stickers[Face.Back] = oldUp;
            Stickers[Face.Down] = oldBack;
            Stickers[Face.Front] = oldDown;
        }

        public void RotateAroundYClockwise()
        {
            Color? oldFront = Stickers[Face.Front];
            Color? oldRight = Stickers[Face.Right];
            Color? oldBack = Stickers[Face.Back];
            Color? oldLeft = Stickers[Face.Left];

            Stickers[Face.Front] = oldLeft;
            Stickers[Face.Right] = oldFront;
            Stickers[Face.Back] = oldRight;
            Stickers[Face.Left] = oldBack;
        }

        public void RotateAroundZClockwise()
        {
            Color? oldUp = Stickers[Face.Up];
            Color? oldRight = Stickers[Face.Right];
            Color? oldDown = Stickers[Face.Down];
            Color? oldLeft = Stickers[Face.Left];

            Stickers[Face.Up] = oldLeft;
            Stickers[Face.Right] = oldUp;
            Stickers[Face.Down] = oldRight;
            Stickers[Face.Left] = oldDown;
        }

        public void RotateAroundXCounterClockwise()
        {
            Color? oldUp = Stickers[Face.Up];
            Color? oldFront = Stickers[Face.Front];
            Color? oldDown = Stickers[Face.Down];
            Color? oldBack = Stickers[Face.Back];

            Stickers[Face.Up] = oldBack;
            Stickers[Face.Front] = oldUp;
            Stickers[Face.Down] = oldFront;
            Stickers[Face.Back] = oldDown;
        }

        public void RotateAroundYCounterClockwise()
        {
            Color? oldFront = Stickers[Face.Front];
            Color? oldRight = Stickers[Face.Right];
            Color? oldBack = Stickers[Face.Back];
            Color? oldLeft = Stickers[Face.Left];

            Stickers[Face.Front] = oldRight;
            Stickers[Face.Left] = oldFront;
            Stickers[Face.Back] = oldLeft;
            Stickers[Face.Right] = oldBack;
        }

        public void RotateAroundZCounterClockwise()
        {
            Color? oldUp = Stickers[Face.Up];
            Color? oldRight = Stickers[Face.Right];
            Color? oldDown = Stickers[Face.Down];
            Color? oldLeft = Stickers[Face.Left];

            Stickers[Face.Up] = oldRight;
            Stickers[Face.Left] = oldUp;
            Stickers[Face.Down] = oldLeft;
            Stickers[Face.Right] = oldDown;
        }
    }
}
