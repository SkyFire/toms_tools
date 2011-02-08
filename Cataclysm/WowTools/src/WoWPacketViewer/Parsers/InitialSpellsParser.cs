using System;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_INITIAL_SPELLS)]
    internal class InitialSpellsParser : Parser
    {
        public InitialSpellsParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            AppendFormatLine("Unk: {0}", gr.ReadByte());

            var spellsCount = gr.ReadUInt16();
            AppendFormatLine("Spells count: {0}", spellsCount);

            for (var i = 0; i < spellsCount; ++i)
            {
                var spellId = gr.ReadUInt32();
                var spellSlot = gr.ReadUInt16();

                AppendFormatLine("Spell: id {0}, slot {1}", spellId, spellSlot);
            }

            var cooldownsCount = gr.ReadUInt16();
            AppendFormatLine("Cooldowns count: {0}", cooldownsCount);

            for (var i = 0; i < cooldownsCount; ++i)
            {
                var spellId = gr.ReadUInt32();
                var itemId = gr.ReadUInt16();
                var category = gr.ReadUInt16();
                var coolDown1 = gr.ReadUInt32();
                var coolDown2 = gr.ReadUInt32();

                AppendFormatLine("Cooldown: spell {0}, item {1}, cat {2}, time1 {3}, time2 {4}", spellId, itemId, category, coolDown1, coolDown2);
            }

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}
