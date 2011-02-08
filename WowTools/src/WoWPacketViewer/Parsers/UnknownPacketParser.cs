namespace WoWPacketViewer.Parsers
{
    internal class UnknownPacketParser : Parser
    {
        public UnknownPacketParser()
            : base(null)
        {
        }

        public override string Parse()
        {
            return string.Empty;
        }
    }
}
