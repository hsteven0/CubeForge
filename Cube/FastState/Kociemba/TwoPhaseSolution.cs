using System.Collections.Generic;
using System.Linq;

namespace CubeForge.Cube.FastState.Kociemba
{
    public class TwoPhaseSolution
    {
        public bool Found { get; set; }
        public List<SearchMove> Phase1Moves { get; set; } = new();
        public List<SearchMove> Phase2Moves { get; set; } = new();

        public int NodesSearched { get; set; }

        public bool HitNodeLimit { get; set; }
        public bool TimedOut { get; set; }
        public string FailureReason { get; set; } = "";

        public List<SearchMove> Moves => Phase1Moves.Concat(Phase2Moves).ToList();

        public static List<SearchMove> InvertMoves(IReadOnlyList<SearchMove> moves)
        {
            List<SearchMove> result = new();
            for (int i = moves.Count - 1; i >= 0; i--)
            {
                SearchMove move = moves[i];
                int inverseTurns = move.Turns switch
                {
                    1 => 3,
                    2 => 2,
                    3 => 1,
                    _ => ((4 - move.Turns) % 4)
                };

                result.Add(new SearchMove(move.Move, inverseTurns));
            }
            return result;
        }
    }
}
