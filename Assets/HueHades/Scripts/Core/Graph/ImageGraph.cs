using System.Collections.Generic;

namespace HueHades.Core.Graph
{
    public class ImageGraph
    {
        private HashSet<GraphNode> _nodes;

        public IEnumerable<GraphNode> Nodes { get { return _nodes; } }


    }
}