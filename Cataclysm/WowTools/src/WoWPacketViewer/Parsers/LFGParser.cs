using System;
using System.Text;
using System.IO;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    enum LfgType
    {
        LFG_TYPE_NONE = 0,
        LFG_TYPE_DUNGEON = 1,
        LFG_TYPE_RAID = 2,
        LFG_TYPE_QUEST = 3,
        LFG_TYPE_ZONE = 4,
        LFG_TYPE_HEROIC_DUNGEON = 5
    };

    [Flags]
    enum LfgRoles
    {
        LEADER = 1,
        TANK = 2,
        HEALER = 4,
        DAMAGE = 8
    };

    //[Parser(OpCodes.MSG_LOOKING_FOR_GROUP)]
    internal class LookingForGroupParser : Parser
    {
        public LookingForGroupParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            switch (Packet.Direction)
            {
                case Direction.Client:
                    {
                        AppendFormatLine("FLG Type: {0}", (LfgType)gr.ReadUInt32());
                        AppendFormatLine("FLG Entry: {0}", gr.ReadUInt32());
                        AppendFormatLine("FLG Unk1: {0}", gr.ReadUInt32());
                    }
                    break;
                case Direction.Server:
                    {
                        ParseServerLookingForGroup(gr);
                    }
                    break;
            }

            CheckPacket(gr);

            return GetParsedString();
        }

        void ParseServerLookingForGroup(BinaryReader gr)
        {
            AppendFormatLine("FLG Type: {0}", (LfgType)gr.ReadUInt32());
            AppendFormatLine("FLG Entry: {0}", gr.ReadUInt32());

            var unk1 = gr.ReadByte();
            AppendFormatLine("unk1: {0}", unk1);

            if (unk1 != 0)
            {
                var count1 = gr.ReadUInt32();
                AppendFormatLine("count1: {0}", count1);

                for (var i = 0; i < count1; ++i)
                {
                    AppendFormatLine("Unk1 GUID {0}: {1:X16}", i, gr.ReadUInt64());
                }
            }

            AppendLine();

            var count2 = gr.ReadUInt32();
            AppendFormatLine("count2: {0}", count2);
            AppendFormatLine("unk2: {0}", gr.ReadUInt32());

            AppendLine();

            for (var i = 0; i < count2; ++i)
            {
                AppendFormatLine("count2 GUID {0}: {1:X16}", i, gr.ReadUInt64());
                var flags = gr.ReadUInt32();
                AppendFormatLine("count2 flags: 0x{0:X8}", flags);

                if ((flags & 0x2) != 0)
                {
                    AppendFormatLine("flags & 0x2 string: {0}", gr.ReadCString());
                }

                if ((flags & 0x10) != 0)
                {
                    AppendFormatLine("flags & 0x10 byte: {0}", gr.ReadByte());
                }

                if ((flags & 0x20) != 0)
                {
                    for (var j = 0; j < 3; ++j)
                        AppendFormatLine("flags & 0x20 byte {0}: {1}", j, gr.ReadByte());
                }

                AppendLine();
            }

            var count3 = gr.ReadUInt32();
            AppendFormatLine("count3: {0}", count3);
            AppendFormatLine("unk3: {0}", gr.ReadUInt32());

            AppendLine();

            for (var i = 0; i < count3; ++i)
            {
                AppendFormatLine("Player GUID: {0:X16}", gr.ReadUInt64());
                var flags = gr.ReadUInt32();
                AppendFormatLine("count3 flags: 0x{0:X8}", flags);

                if ((flags & 0x1) != 0)
                {
                    AppendFormatLine("Level: {0}", gr.ReadByte());
                    AppendFormatLine("Class: {0}", gr.ReadByte());
                    AppendFormatLine("Race: {0}", gr.ReadByte());

                    for (var j = 0; j < 3; ++j)
                        AppendFormatLine("talents in tab {0}: {1}", j, gr.ReadByte());

                    AppendFormatLine("resistances1: {0}", gr.ReadUInt32());
                    AppendFormatLine("spd/heal: {0}", gr.ReadUInt32());
                    AppendFormatLine("spd/heal: {0}", gr.ReadUInt32());
                    AppendFormatLine("combat_rating9: {0}", gr.ReadUInt32());
                    AppendFormatLine("combat_rating10: {0}", gr.ReadUInt32());
                    AppendFormatLine("combat_rating11: {0}", gr.ReadUInt32());
                    AppendFormatLine("mp5: {0}", gr.ReadSingle());
                    AppendFormatLine("flags & 0x1 float: {0}", gr.ReadSingle());
                    AppendFormatLine("attack power: {0}", gr.ReadUInt32());
                    AppendFormatLine("stat1: {0}", gr.ReadUInt32());
                    AppendFormatLine("maxhealth: {0}", gr.ReadUInt32());
                    AppendFormatLine("maxpower1: {0}", gr.ReadUInt32());
                    AppendFormatLine("flags & 0x1 uint: {0}", gr.ReadUInt32());
                    AppendFormatLine("flags & 0x1 float: {0}", gr.ReadSingle());
                    AppendFormatLine("flags & 0x1 uint: {0}", gr.ReadUInt32());
                    AppendFormatLine("flags & 0x1 uint: {0}", gr.ReadUInt32());
                    AppendFormatLine("flags & 0x1 uint: {0}", gr.ReadUInt32());
                    AppendFormatLine("flags & 0x1 uint: {0}", gr.ReadUInt32());
                    AppendFormatLine("combat_rating20: {0}", gr.ReadUInt32());
                    AppendFormatLine("flags & 0x1 uint: {0}", gr.ReadUInt32());
                }

                if ((flags & 0x2) != 0)
                {
                    AppendFormatLine("Comment: {0}", gr.ReadCString());
                }

                if ((flags & 0x4) != 0)
                {
                    AppendFormatLine("flags & 0x4 byte: {0}", gr.ReadByte());
                }

                if ((flags & 0x8) != 0)
                {
                    AppendFormatLine("flags & 0x8 guid: {0:X16}", gr.ReadUInt64());
                }

                if ((flags & 0x10) != 0)
                {
                    AppendFormatLine("flags & 0x10 byte: {0}", gr.ReadByte());
                }

                if ((flags & 0x20) != 0)
                {
                    AppendFormatLine("Roles: {0}", (LfgRoles)gr.ReadByte());
                }

                if ((flags & 0x40) != 0)
                {
                    AppendFormatLine("AreaId: {0}", gr.ReadUInt32());
                }

                if ((flags & 0x100) != 0)
                {
                    AppendFormatLine("flags & 0x100 byte: {0}", gr.ReadByte());
                }

                if ((flags & 0x80) != 0)
                {
                    for (var j = 0; j < 3; ++j)
                    {
                        var temp = gr.ReadUInt32();
                        var type = (temp >> 24) & 0x000000FF;
                        var entry = temp & 0x00FFFFFF;
                        AppendFormatLine("LFG Slot {0}: LFG Type: {1}, LFG Entry: {2}", j, (LfgType)type, entry);
                    }
                }

                AppendLine();
            }
        }
    }
}
