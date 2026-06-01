using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeForge.Cube
{
    public static class Scrambler
    {
        private static readonly Random random = new();

        private static readonly string[] Faces = { "R", "L", "U", "D", "F", "B" };
        private static readonly string[] Suffixes = { "", "'", "2" };

        // A random move scrambleManager generator
        public static string GenerateScramble(int length = 20)
        {
            List<string> moves = new();
            string? previousFace = null;
            string? previousAxis = null;

            while (moves.Count < length)
            {
                string face = Faces[random.Next(Faces.Length)];
                string axis = GetAxis(face);

                // Avoid same face twice in a row
                if (face == previousFace)
                    continue;

                // Avoid same axis twice in a row
                if (axis == previousAxis)
                    continue;

                string suffix = Suffixes[random.Next(Suffixes.Length)];
                moves.Add(face + suffix);

                previousFace = face;
                previousAxis = axis;
            }

            return string.Join(" ", moves);
        }

        private static string GetAxis(string face)
        {
            return face switch
            {
                "R" or "L" => "X",
                "U" or "D" => "Y",
                "F" or "B" => "Z",
                _ => ""
            };
        }
    }
}
