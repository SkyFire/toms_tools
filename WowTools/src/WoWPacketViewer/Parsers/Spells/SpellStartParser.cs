using WowTools.Core;

namespace WoWPacketViewer.Parsers.Spells
{
    [Parser(OpCodes.SMSG_SPELL_START)]
    internal class SpellStartParser : SpellParserBase
    {
        public SpellStartParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            AppendFormatLine("Caster: 0x{0:X16}", gr.ReadPackedGuid());
            AppendFormatLine("Target: 0x{0:X16}", gr.ReadPackedGuid());
            AppendFormatLine("Pending Cast: {0}", gr.ReadByte());
            AppendFormatLine("Spell Id: {0}", gr.ReadUInt32());
            var cf = (CastFlags)gr.ReadUInt32();
            AppendFormatLine("Cast Flags: {0}", cf);
            AppendFormatLine("Timer: {0}", gr.ReadUInt32());

            ReadTargets(gr);

            if (cf.HasFlag(CastFlags.CAST_FLAG_12))
            {
                AppendFormatLine("PredictedPower: {0}", gr.ReadUInt32());
            }

            if (cf.HasFlag(CastFlags.CAST_FLAG_22))
            {
                var v1 = gr.ReadByte();
                AppendFormatLine("RuneState Before: {0}", (CooldownMask)v1);
                var v2 = gr.ReadByte();
                AppendFormatLine("RuneState Now: {0}", (CooldownMask)v2);

                for (var i = 0; i < 6; ++i)
                {
                    var v3 = (i << i);

                    if ((v3 & v1) != 0)
                    {
                        if ((v3 & v2) == 0)
                        {
                            var v4 = gr.ReadByte();
                            AppendFormatLine("Cooldown for {0} is {1}", (CooldownMask)v3, v4);
                        }
                    }
                }
            }

            if (cf.HasFlag(CastFlags.CAST_FLAG_06))
            {
                AppendFormatLine("Projectile displayid {0}, inventoryType {1}", gr.ReadUInt32(), gr.ReadUInt32());
            }

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}