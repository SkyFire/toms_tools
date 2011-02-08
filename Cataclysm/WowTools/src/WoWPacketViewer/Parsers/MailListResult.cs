using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_MAIL_LIST_RESULT)]
    internal class MailListResult : Parser
    {
        public MailListResult(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            var realCount = gr.ReadUInt32();

            AppendFormatLine("Real Count: {0}", realCount);

            var displayCount = gr.ReadByte();

            AppendFormatLine("Displayed Count: {0}", displayCount);

            for (var i = 0; i < displayCount; ++i)
            {
                var len = gr.ReadUInt16();
                var id = gr.ReadUInt32();
                var type = gr.ReadByte();

                AppendFormatLine("Message {0}: data len {1}, id {2}, type {3}", i, len, id, type);

                switch(type)
                {
                    case 0:
                        {
                            var guid = gr.ReadUInt64();
                            AppendFormatLine("Sender guid: {0:X16}", guid);
                            break;
                        }
                    default:
                        {
                            var entry = gr.ReadUInt32();
                            AppendFormatLine("Sender entry: {0:X16}", entry);
                            break;
                        }
                }

                var cod = gr.ReadUInt32();
                AppendFormatLine("COD: {0}", cod);
                var itemTextId = gr.ReadUInt32();
                AppendFormatLine("Item Text Id: {0}", itemTextId);
                var stationary = gr.ReadUInt32();
                AppendFormatLine("Stationary: {0}", stationary);
                var money = gr.ReadUInt32();
                AppendFormatLine("Money: {0}", money);
                var flags = gr.ReadUInt32();
                AppendFormatLine("Flags: {0}", flags);
                var time = gr.ReadSingle();
                AppendFormatLine("Time: {0}", time);
                var templateId = gr.ReadUInt32();
                AppendFormatLine("Template Id: {0}", templateId);
                var subject = gr.ReadCString();
                AppendFormatLine("Subject: {0}", subject);
                var body = gr.ReadCString();
                AppendFormatLine("Body: {0}", body);

                var itemsCount = gr.ReadByte();
                AppendFormatLine("Items Count: {0}", itemsCount);

                for(var j = 0; j < itemsCount; ++j)
                {
                    AppendFormatLine("Item: {0}", j);
                    var slot = gr.ReadByte();
                    var guid = gr.ReadUInt32();
                    var entry = gr.ReadUInt32();

                    AppendFormatLine("Slot {0}, guid {1}, entry {2}", slot, guid, entry);

                    for(var k = 0; k < 7; ++k)
                    {
                        var charges = gr.ReadUInt32();
                        var duration = gr.ReadUInt32();
                        var enchid = gr.ReadUInt32();

                        AppendFormatLine("Enchant {0}: charges {1}, duration {2}, id {3}", k, charges, duration, enchid);
                    }

                    var randomProperty = gr.ReadUInt32();
                    AppendFormatLine("Random Property: {0}", randomProperty);
                    var itemSuffixFactor = gr.ReadUInt32();
                    AppendFormatLine("Item Suffix Factor: {0}", itemSuffixFactor);
                    var stackCount = gr.ReadUInt32();
                    AppendFormatLine("Stack Count: {0}", stackCount);
                    var spellCharges = gr.ReadUInt32();
                    AppendFormatLine("Spell Charges: {0}", spellCharges);
                    var maxDurability = gr.ReadUInt32();
                    var durability = gr.ReadUInt32();
                    AppendFormatLine("Durability: {0} (max {1})", durability, maxDurability);
                    var unk = gr.ReadByte();
                    AppendFormatLine("Unk: {0}", unk);

                    AppendLine();
                }

                AppendLine();
            }

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}
