
namespace HueHades.Core.Graph
{
    public class NodePort<T> : INodePort
    {
        private string _portID;
        private string _portName;

        public virtual string PortID {
            get { return _portID; }
        }
        public virtual string PortName
        {
            get { return _portName; }
        }

        public NodePort(string portID, string portDisplayName, T initialNodeValue){
            _portID = portID;
            _portName = portDisplayName;
         }


    }
}