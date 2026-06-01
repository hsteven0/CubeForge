using System;
using System.Collections.Generic;

namespace CubeForge.Cube
{
    // Represents one parsed cube move (HMT).
    // Example: R = Move=R, Turns=1
    public class ParsedMove
    {
        public FaceMove Move { get; }
        public int Turns { get; }

        // True for moves written like R2', U2', r2', etc.
        public bool DoublePrime { get; }

        public ParsedMove(FaceMove move, int turns, bool doublePrime = false)
        {
            Move = move;
            Turns = turns;
            DoublePrime = doublePrime;
        }

        public override string ToString()
        {
            string moveText = Move switch
            {
                FaceMove.Rw => "r",
                FaceMove.Lw => "l",
                FaceMove.Uw => "u",
                FaceMove.Dw => "d",
                FaceMove.Fw => "f",
                FaceMove.Bw => "b",
                FaceMove.X => "x",
                FaceMove.Y => "y",
                FaceMove.Z => "z",
                _ => Move.ToString()
            };

            string suffix = Turns switch
            {
                2 => DoublePrime ? "2'" : "2",
                3 => "'",
                _ => ""
            };

            return moveText + suffix;
        }
    }

    public static class ScrambleParser
    {
        public static List<ParsedMove> Parse(string scramble)
        {
            var moves = new List<ParsedMove>();

            if (string.IsNullOrWhiteSpace(scramble))
                return moves;

            string[] tokens = scramble.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string token in tokens)
            {
                FaceMove move = ParseMoveName(token);

                int turns = 1;
                bool doublePrime = false;

                if (token.Contains("2"))
                {
                    turns = 2;
                    doublePrime = token.Contains("2'");
                }
                else if (token.EndsWith("'"))
                {
                    turns = 3;
                }

                moves.Add(new ParsedMove(move, turns, doublePrime));
            }

            return moves;
        }

        private static FaceMove ParseMoveName(string token)
        {
            string movePart = token
                .Replace("2", "")
                .Replace("'", "");

            return movePart switch
            {
                "R" => FaceMove.R,
                "L" => FaceMove.L,
                "U" => FaceMove.U,
                "D" => FaceMove.D,
                "F" => FaceMove.F,
                "B" => FaceMove.B,

                "r" or "Rw" => FaceMove.Rw,
                "l" or "Lw" => FaceMove.Lw,
                "u" or "Uw" => FaceMove.Uw,
                "d" or "Dw" => FaceMove.Dw,
                "f" or "Fw" => FaceMove.Fw,
                "b" or "Bw" => FaceMove.Bw,

                "M" => FaceMove.M,
                "E" => FaceMove.E,
                "S" => FaceMove.S,

                "x" => FaceMove.X,
                "y" => FaceMove.Y,
                "z" => FaceMove.Z,

                _ => throw new ArgumentException($"Invalid move: {token}")
            };
        }
    }
}