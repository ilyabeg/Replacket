namespace ReplacketModel.Events
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public byte[] PacketData { get; }
        public int PacketIndex { get; }
        public int PacketLength { get; }
        public string PacketLinkLayer { get; }

        public PacketReceivedEventArgs(byte[] packetData, int length, string linkLayer, int index)
        {
            PacketData = packetData;
            PacketLength = length;
            PacketLinkLayer = linkLayer;
            PacketIndex = index;
        }
    }
}
