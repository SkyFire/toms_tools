namespace WoWPacketViewer
{
    public static class PacketReaderFactory
    {
        public static IPacketReader Create(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".bin":
                case ".pkt":
                    return new WowCorePacketReader();
                case ".sqlite":
                    return new SqLitePacketReader();
                case ".xml":
                    return new SniffitztPacketReader();
                default:
                    return null;
            }
        }
    }
}
