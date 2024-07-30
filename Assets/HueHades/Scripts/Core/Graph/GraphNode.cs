using System.Collections.Generic;
using Unity.Mathematics;

namespace HueHades.Core.Graph
{
    public abstract class GraphNode
    {
        private List<INodePort> _inputNodes;
        private List<INodePort> _outputNodes;

        public IEnumerable<INodePort> InputNodes => _inputNodes;
        public IEnumerable<INodePort> OutputNodes => _outputNodes;

        public float2 _nodeOffset;

        public float2 NodeOffset { get => _nodeOffset; set { _nodeOffset = value; } }

        public abstract string DisplayName { get; set; } 

        public abstract string NodeID { get; set; }

        protected GraphNode() { 
            _inputNodes = RegisterInputNodes();
            _outputNodes = RegisterOutputNodes();

            if (_inputNodes == null) _inputNodes = new List<INodePort>();
            if (_outputNodes == null) _outputNodes = new List<INodePort>();
        }


        /// <summary>
        /// Return a list of ports you wish to register as input ports
        /// </summary>
        /// <returns></returns>
        protected abstract List<INodePort> RegisterInputNodes();

        /// <summary>
        /// Return a list of ports you wish to register as output ports
        /// </summary>
        /// <returns></returns>
        protected abstract List<INodePort> RegisterOutputNodes();
    }
}