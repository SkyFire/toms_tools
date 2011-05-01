using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_TUTORIAL_FLAGS)]
    internal class PowerUpdateParser : Parser
    {
        public PowerUpdateParser(Packet pkt)
            : base(pkt)
        {
        }

        public override void Parse()
        {
            var gr = Packet.CreateReader();

            for(var i = 0; i < 8; ++i)
                AppendFormatLine("Mask {0}: {1:X8}", i, gr.ReadUInt32());

            CheckPacket(gr);
        }
    }
}
