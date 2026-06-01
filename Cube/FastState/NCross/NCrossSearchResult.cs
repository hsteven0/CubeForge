using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeForge.Cube.FastState
{
    public class NCrossSearchResult
    {
        public bool Found { get; set; }

        public NCrossTarget Target { get; set; } = new NCrossTarget();

        public List<SearchMove> Solution { get; set; } = new();

        public int NodesSearched { get; set; }
    }
}