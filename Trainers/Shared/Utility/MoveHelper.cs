using System.Collections.Generic;
using System.Linq;
using CubeForge.Cube;
using CubeForge.Cube.FastState;

namespace CubeForge.Trainers.Shared
{
    public static class MoveHelper
    {
        public static List<SearchMove> ToSearchMoves(IEnumerable<ParsedMove> moves)
        {
            return moves.Select(m => new SearchMove(m.Move, m.Turns)).ToList();
        }

        public static List<SearchMove> ParseSearchMoves(string algorithm)
        {
            if (string.IsNullOrWhiteSpace(algorithm))
                return new List<SearchMove>();

            return ToSearchMoves(ScrambleParser.Parse(algorithm));
        }

        public static string MovesToText(IEnumerable<SearchMove> moves)
        {
            return string.Join(" ", moves);
        }

        public static void ApplyMoves(FastCubeState state, IEnumerable<SearchMove> moves)
        {
            foreach (SearchMove move in moves)
            {
                if (move.Turns != 0)
                    state.ApplyMove(move.Move, move.Turns);
            }
        }

        public static bool SameMoveText(IEnumerable<SearchMove> a, IEnumerable<SearchMove> b)
        {
            return NormalizeMoveText(a).Equals(
                NormalizeMoveText(b),
                StringComparison.OrdinalIgnoreCase);
        }

        public static string NormalizeMoveText(IEnumerable<SearchMove> moves)
        {
            return string.Join(" ", moves.Select(m => m.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}
