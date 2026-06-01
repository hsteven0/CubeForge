using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeForge.Cube.FastState
{
    public class SearchMove
    {
        public FaceMove Move { get; }
        public int Turns { get; }

        public SearchMove(FaceMove move, int turns)
        {
            Move = move;
            Turns = turns;
        }

        public override string ToString()
        {
            return Turns switch
            {
                1 => Move.ToString(),
                2 => Move + "2",
                3 => Move + "'",
                _ => Move.ToString()
            };
        }
    }
}