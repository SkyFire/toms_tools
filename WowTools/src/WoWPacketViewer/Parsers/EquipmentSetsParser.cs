using System;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_EQUIPMENT_SET_LIST)]
    internal class EquipmentSetListParser : Parser
    {
        public EquipmentSetListParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            var count = gr.ReadUInt32();
            AppendFormatLine("Count: {0}", count);

            for (var i = 0; i < count; ++i)
            {
                var setguid = gr.ReadPackedGuid();
                var setindex = gr.ReadUInt32();
                var name = gr.ReadCString();
                var iconname = gr.ReadCString();

                AppendFormatLine("EquipmentSet {0}: guid {1}, index {2}, name {3}, iconname {4}", i, setguid, setindex, name, iconname);

                for (var j = 0; j < 19; ++j)
                    AppendFormatLine("EquipmentSetItem {0}: guid {1}", j, gr.ReadPackedGuid().ToString("X16"));
            }

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}
