using System.Net;

namespace EcsCore.Network.NetworkSocket
{
    public struct NetworkArrivedData
    {
        public EndPoint EndPoint;
        public byte[] Data;
    }
}