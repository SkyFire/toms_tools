using System;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_POWER_UPDATE)]
    internal class PowerUpdateParser : Parser
    {
        private enum PowerType
        {
            Mana,
            Rage,
            Focus,
            Energy,
            Happiness,
            Rune,
            RunicPower,
        }

        public PowerUpdateParser(Packet pkt)
            : base(pkt)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            AppendFormatLine("GUID: 0x{0:X16}", gr.ReadPackedGuid());
            AppendFormatLine("Type: {0}", (PowerType)gr.ReadByte());
            AppendFormatLine("Value: {0}", gr.ReadUInt32());

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}
