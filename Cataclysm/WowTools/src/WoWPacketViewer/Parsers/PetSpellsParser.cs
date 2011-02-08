using System;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_PET_SPELLS)]
    internal class PetSpellsParser : Parser
    {
        public PetSpellsParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            var petGUID = gr.ReadUInt64();
            AppendFormatLine("GUID: {0:X16}", petGUID);

            if (petGUID != 0)
            {
                AppendFormatLine("Pet family: {0}", gr.ReadUInt16());

                var unk1 = gr.ReadUInt32();
                var unk2 = gr.ReadUInt32();
                AppendFormatLine("Unk1: {0}, Unk2: {1:X8}", unk1, unk2);

                for (var i = 0; i < 10; ++i)
                {
                    var spellOrAction = gr.ReadUInt16();
                    var type = gr.ReadUInt16();

                    AppendFormatLine("SpellOrAction: id {0}, type {1:X4}", spellOrAction, type);
                }

                var spellsCount = gr.ReadByte();
                AppendFormatLine("Spells count: {0}", spellsCount);

                for (var i = 0; i < spellsCount; ++i)
                {
                    var spellId = gr.ReadUInt16();
                    var active = gr.ReadUInt16();

                    AppendFormatLine("Spell {0}, active {1:X4}", spellId, active);
                }

                var cooldownsCount = gr.ReadByte();
                AppendFormatLine("Cooldowns count: {0}", cooldownsCount);

                for (var i = 0; i < cooldownsCount; ++i)
                {
                    var spell = gr.ReadUInt32();
                    var category = gr.ReadUInt16();
                    var cooldown = gr.ReadUInt32();
                    var categoryCooldown = gr.ReadUInt32();

                    AppendFormatLine("Cooldown: spell {0}, category {1}, cooldown {2}, categoryCooldown {3}", spell, category, cooldown, categoryCooldown);
                }
            }

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}
