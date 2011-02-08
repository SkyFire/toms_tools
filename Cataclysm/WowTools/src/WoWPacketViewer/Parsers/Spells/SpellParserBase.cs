using System.IO;
using WowTools.Core;

namespace WoWPacketViewer.Parsers.Spells
{
    internal abstract class SpellParserBase : Parser
    {
        protected SpellParserBase(Packet packet)
            : base(packet)
        {
        }

        protected TargetFlags ReadTargets(BinaryReader br)
        {
            var tf = (TargetFlags) br.ReadUInt32();
            AppendFormatLine("TargetFlags: {0}", tf);

            if (tf.HasFlag(TargetFlags.TARGET_FLAG_UNIT) ||
                tf.HasFlag(TargetFlags.TARGET_FLAG_PVP_CORPSE) ||
                tf.HasFlag(TargetFlags.TARGET_FLAG_OBJECT) ||
                tf.HasFlag(TargetFlags.TARGET_FLAG_CORPSE) ||
                tf.HasFlag(TargetFlags.TARGET_FLAG_UNK2))
            {
                AppendFormatLine("ObjectTarget: 0x{0:X16}", br.ReadPackedGuid());
            }

            if (tf.HasFlag(TargetFlags.TARGET_FLAG_ITEM) ||
                tf.HasFlag(TargetFlags.TARGET_FLAG_TRADE_ITEM))
            {
                AppendFormatLine("ItemTarget: 0x{0:X16}", br.ReadPackedGuid());
            }

            if (tf.HasFlag(TargetFlags.TARGET_FLAG_SOURCE_LOCATION))
            {
                AppendFormatLine("SrcTarget: {0} {1} {2}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            }

            if (tf.HasFlag(TargetFlags.TARGET_FLAG_DEST_LOCATION))
            {
                AppendFormatLine("DstTargetGuid: {0}", br.ReadPackedGuid().ToString("X16"));
                AppendFormatLine("DstTarget: {0} {1} {2}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            }

            if (tf.HasFlag(TargetFlags.TARGET_FLAG_STRING))
            {
                AppendFormatLine("StringTarget: {0}", br.ReadCString());
            }

            return tf;
        }
    }
}