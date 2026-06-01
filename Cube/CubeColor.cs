using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CubeForge.Cube
{
    public enum CubeColor
    {
        White,
        Yellow,
        Red,
        Orange,
        Blue,
        Green
    }

    public static class CubeColorHelper
    {
        // Gets the solved cube direction vector for a color
        public static Vector3D GetSolvedDirection(CubeColor color)
        {
            return color switch
            {
                CubeColor.White => new Vector3D(0, 1, 0),   // Up
                CubeColor.Yellow => new Vector3D(0, -1, 0), // Down
                CubeColor.Red => new Vector3D(0, 0, 1),     // Front
                CubeColor.Orange => new Vector3D(0, 0, -1), // Back
                CubeColor.Blue => new Vector3D(1, 0, 0),    // Right
                CubeColor.Green => new Vector3D(-1, 0, 0),  // Left
                _ => new Vector3D(0, 1, 0)
            };
        }

        // Gets the opposite face color on the cube
        public static CubeColor GetOpposite(CubeColor color)
        {
            return color switch
            {
                CubeColor.White => CubeColor.Yellow,
                CubeColor.Yellow => CubeColor.White,
                CubeColor.Red => CubeColor.Orange,
                CubeColor.Orange => CubeColor.Red,
                CubeColor.Blue => CubeColor.Green,
                CubeColor.Green => CubeColor.Blue,
                _ => CubeColor.White
            };
        }
    }
}