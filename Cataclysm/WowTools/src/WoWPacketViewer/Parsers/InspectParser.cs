using System;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_INSPECT_TALENT)]
    internal class InspectTalentParser : Parser
    {
        public InspectTalentParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            AppendFormatLine("GUID: {0:X16}", gr.ReadPackedGuid());
            AppendFormatLine("Free talent points: {0}", gr.ReadUInt32());
            var talentGroupsCount = gr.ReadByte();
            AppendFormatLine("Talent groups count: {0}", talentGroupsCount);
            AppendFormatLine("Talent group index: {0}", gr.ReadByte());

            if (talentGroupsCount > 0)
            {
                var talentsCount = gr.ReadByte();
                AppendFormatLine("Talents count {0}", talentsCount);

                for (var i = 0; i < talentsCount; ++i)
                {
                    AppendFormatLine("Talent {0}: id {1}, rank {2}", i, gr.ReadUInt32(), gr.ReadByte());
                }

                var glyphsCount = gr.ReadByte();
                AppendFormatLine("Glyphs count {0}", glyphsCount);

                for (var i = 0; i < glyphsCount; ++i)
                {
                    AppendFormatLine("Glyph {0}: id {1}", i, gr.ReadUInt16());
                }
            }

            var slotUsedMask = gr.ReadUInt32();

            for (var i = 0; i < 19; ++i)    // max equip slot
            {
                if (((1 << i) & slotUsedMask) != 0)
                {
                    AppendFormatLine("Item {0}: entry {1}", i, gr.ReadUInt32());

                    var enchantmentMask = gr.ReadUInt16();

                    for (var j = 0; j < 12; ++j)    // max enchantments
                    {
                        if (((1 << j) & enchantmentMask) != 0)
                        {
                            AppendFormatLine("Item {0}: enchant {1}, id {2}", i, j, gr.ReadUInt16());
                        }
                    }

                    AppendFormatLine("Item {0}: unk1 {1:X4}", i, gr.ReadUInt16());
                    AppendFormatLine("Item {0}: unk2 {1:X16}", i, gr.ReadPackedGuid());
                    AppendFormatLine("Item {0}: unk3 {1:X8}", i, gr.ReadUInt32());
                }
            }

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}
