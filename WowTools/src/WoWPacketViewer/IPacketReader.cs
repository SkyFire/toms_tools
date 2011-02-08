using System.Collections.Generic;

namespace WoWPacketViewer
{
    public interface IPacketReader
    {
        uint Build { get; }
        IEnumerable<Packet> ReadPackets(string file);
    }
}
