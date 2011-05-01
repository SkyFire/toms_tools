using System;
using System.Text;
using System.IO;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.CMSG_AUTH_SESSION)]
    internal class CMSG_AUTH_SESSION : Parser
    {
        public CMSG_AUTH_SESSION(Packet packet)
            : base(packet)
        {
        }

        public override void Parse()
        {
            var gr = Packet.CreateReader();

            var clientBuild = gr.ReadUInt32();
            var unk1 = gr.ReadUInt32();
            var account = gr.ReadCString();
            var unk2 = gr.ReadUInt32();
            var clientSeed = gr.ReadUInt32();
            var unk4 = gr.ReadUInt32();
            var unk5 = gr.ReadUInt32();
            var unk6 = gr.ReadUInt32();
            var unk3 = gr.ReadUInt64();
            var digest = gr.ReadBytes(20);

            AppendFormatLine("Client Build: {0}", clientBuild);
            AppendFormatLine("Unk1: {0}", unk1);
            AppendFormatLine("Account: {0}", account);
            AppendFormatLine("Unk2: {0}", unk2);
            AppendFormatLine("Client Seed: {0}", clientSeed);
            AppendFormatLine("Unk4: {0}", unk4);
            AppendFormatLine("Unk5: {0}", unk5);
            AppendFormatLine("Unk6: {0}", unk6);
            AppendFormatLine("Unk3: {0}", unk3);
            AppendFormatLine("Digest: {0}", digest.ToHexString());

            // addon info
            var addonData = gr.ReadBytes((int)gr.BaseStream.Length - (int)gr.BaseStream.Position);
            var decompressed = addonData.Decompress();

            AppendFormatLine("Decompressed addon data:");
            AppendFormatLine(decompressed.HexLike(0, decompressed.Length));

            using (var reader = new BinaryReader(new MemoryStream(decompressed)))
            {
                var count = reader.ReadUInt32();
                AppendFormatLine("Addons Count: {0}", count);
                for (var i = 0; i < count; ++i)
                {
                    var addonName = reader.ReadCString();
                    var enabled = reader.ReadByte();
                    var crc = reader.ReadUInt32();
                    var unk7 = reader.ReadUInt32();
                    AppendFormatLine("Addon {0}: name {1}, enabled {2}, crc {3}, unk7 {4}", i, addonName, enabled, crc, unk7);
                }

                var unk8 = reader.ReadUInt32();
                AppendFormatLine("Unk5: {0}", unk8);
            }
            // addon info end

            CheckPacket(gr);
        }
    }
}
