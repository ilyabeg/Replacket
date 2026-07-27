namespace ReplacketModel.Events
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public byte[] PacketData { get; }
        public int PacketIndex { get; }
        public string PacketProtocol { get; }
        public int PacketLength { get; }
        public string PacketLinkLayer { get; }

        public PacketReceivedEventArgs(byte[] packetData, string protocol, int length, string linkLayer, int index)
        {
            PacketData = packetData;
            PacketProtocol = protocol;
            PacketLength = length;
            PacketLinkLayer = linkLayer;
            PacketIndex = index;
        }
    }
}
