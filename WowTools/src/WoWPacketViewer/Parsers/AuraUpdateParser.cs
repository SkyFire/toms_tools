using System;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_AURA_UPDATE)]
    [Parser(OpCodes.SMSG_AURA_UPDATE_ALL)]
    internal class AuraUpdateParser : Parser
    {
        [Flags]
        private enum AuraFlags : byte
        {
            None = 0x00,
            Index1 = 0x01,
            Index2 = 0x02,
            Index3 = 0x04,
            NotOwner = 0x08,
            Positive = 0x10,
            Duration = 0x20,
            Unk1 = 0x40,
            Negative = 0x80,
        }

        public AuraUpdateParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            AppendFormatLine("GUID: {0:X16}", gr.ReadPackedGuid());

            while (gr.BaseStream.Position < gr.BaseStream.Length)
            {
                AppendFormatLine("Slot: {0:X2}", gr.ReadByte());

                var spellId = gr.ReadUInt32();
                AppendFormatLine("Spell: {0:X8}", spellId);

                if (spellId > 0)
                {
                    var af = (AuraFlags)gr.ReadByte();
                    AppendFormatLine("Flags: {0}", af);

                    AppendFormatLine("Level: {0:X2}", gr.ReadByte());

                    AppendFormatLine("Charges: {0:X2}", gr.ReadByte());

                    if (af.HasFlag(AuraFlags.NotOwner) == false)
                    {
                        AppendFormatLine("GUID2: {0:X16}", gr.ReadPackedGuid());
                    }

                    if (af.HasFlag(AuraFlags.Duration))
                    {
                        AppendFormatLine("Full duration: {0:X8}", gr.ReadUInt32());

                        AppendFormatLine("Rem. duration: {0:X8}", gr.ReadUInt32());
                    }
                }
                AppendLine();
            }

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}
