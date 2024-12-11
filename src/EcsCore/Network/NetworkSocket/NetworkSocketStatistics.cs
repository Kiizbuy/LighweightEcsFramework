namespace EcsCore.Network.NetworkSocket
{
    public struct NetworkSocketStatistics
    {
        public ulong BytesSent;
        public ulong BytesReceived;

        public static NetworkSocketStatistics Create()
        {
            return new NetworkSocketStatistics();
        }
    }
}