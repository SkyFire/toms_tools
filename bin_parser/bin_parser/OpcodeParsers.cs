using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Defines;
using WoWReader;

namespace OpcodeParsers
{
    public class OpcodeParser
    {
        static uint MapId
        {
            get
            {
                return MapId;
            }
            set
            {
                MapId = value;
            }
        }

        OpcodeParser()
        {
            MapId = 0;
        }

        /// <summary>
        /// Monster move opcode parser method.
        /// </summary>
        /// <param name="gr">Main stream reader.</param>
        /// <param name="gr2">Packet stream reader.</param>
        /// <param name="sb">Logger string builder.</param>
        /// <param name="swe">Error logger writer.</param>
        /// <returns>Successful</returns>
        public static bool ParseMonsterMoveOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            sb.AppendLine("Packet offset " + gr.BaseStream.Position.ToString("X2"));
            sb.AppendLine("Opcode SMSG_MONSTER_MOVE (0x00DD)");

            ulong guid = gr2.ReadPackedGuid();
            sb.AppendLine("GUID " + guid.ToString("X16"));

            Coords3 coords = gr2.ReadCoords3();
            sb.AppendLine("Start point " + coords.GetCoords());

            uint time = gr2.ReadUInt32();
            sb.AppendLine("Time " + time);

            byte unk = gr2.ReadByte();
            sb.AppendLine("unk_byte " + unk);

            switch (unk)
            {
                case 0: // обычный пакет
                    break;
                case 1: // стоп, конец пакета...
                    sb.AppendLine("stop");
                    return true;
                case 2:
                    Coords3 point = gr2.ReadCoords3();
                    sb.AppendLine("unk point " + point.GetCoords());
                    break;
                case 3: // чей-то гуид, скорее всего таргета...
                    ulong target_guid = gr2.ReadUInt64();
                    sb.AppendLine("GUID unknown " + target_guid.ToString("X16"));
                    break;
                case 4: // похоже на ориентацию...
                    float orientation = gr2.ReadSingle();
                    sb.AppendLine("Orientation " + orientation.ToString().Replace(",", "."));
                    break;
                default:
                    swe.WriteLine("Error in position " + gr.BaseStream.Position.ToString("X2"));
                    swe.WriteLine("unknown unk " + unk);
                    break;
            }

            Flags flags = (Flags)gr2.ReadUInt32();
            sb.AppendLine("Flags " + flags);

            uint movetime = gr2.ReadUInt32();
            sb.AppendLine("MoveTime " + movetime);

            uint points = gr2.ReadUInt32();
            sb.AppendLine("Points " + points);

            List<Node> nodes = new List<Node>((int)points);

            if ((flags & Flags.flag10) != 0) // 0x200
            {
                sb.AppendLine("Taxi");
                for (uint i = 0; i < points; i++)
                {
                    Node node = new Node();
                    node.x = gr2.ReadSingle();
                    node.y = gr2.ReadSingle();
                    node.z = gr2.ReadSingle();
                    nodes.Add(node);
                    //Coords3 path = gr2.ReadCoords3();
                    //sb.AppendLine("Path point" + i + ": " + path.GetCoords());
                }
            }
            else
            {
                if ((flags & Flags.flag09) == 0 && (flags & Flags.flag10) == 0 && flags != 0)
                {
                    swe.WriteLine("Unknown flags " + flags);
                }

                if ((flags & Flags.flag09) != 0)
                    sb.AppendLine("Running");

                Coords3 end = gr2.ReadCoords3();
                sb.AppendLine("End point " + end.GetCoords());

                for (uint i = 0; i < (points - 1); i++)
                {
                    int mask = gr2.ReadInt32();
                    sb.AppendLine("shift mask" + i + " " + mask.ToString("X8"));

                    int temp1, temp2, temp3;
                    temp1 = (mask & 0x07FF) << 0x15;
                    temp2 = ((mask >> 0x0B) & 0x07FF) << 0x15;
                    temp3 = (mask >> 0x16) << 0x16;
                    temp1 >>= 0x15;
                    temp2 >>= 0x15;
                    temp3 >>= 0x16;
                    float x = temp1 * 0.25f;
                    float y = temp2 * 0.25f;
                    float z = temp3 * 0.25f;
                    sb.AppendLine("shift is " + x + " " + y + " " + z + ".");
                }
            }

            if ((flags & Flags.flag10) != 0)
            {
                StreamWriter sw = new StreamWriter("taxiinfo.txt", true);
                sw.WriteLine("GUID: 0x" + guid.ToString("X16"));
                sw.WriteLine(string.Format("Position: {0} {1} {2}", coords.X, coords.Y, coords.Z));
                sw.WriteLine("Time: " + time);
                sw.WriteLine("Movetime: " + movetime);
                sw.WriteLine("Nodes: " + points);
                for (int i = 0; i < points; i++)
                    sw.WriteLine(string.Format("Node {0}: {1} {2} {3}", i, nodes[i].x, nodes[i].y, nodes[i].z));

                uint mangos_time = 0;

                float len = 0, xd, yd, zd;

                /*xd = nodes[0].x - coords.X;
                yd = nodes[0].y - coords.Y;
                zd = nodes[0].z - coords.Z;
                len += (float)Math.Sqrt((xd * xd + yd * yd + zd * zd));*/

                for (int i = 1; i < points; i++)
                {
                    xd = nodes[i].x - nodes[i - 1].x;
                    yd = nodes[i].y - nodes[i - 1].y;
                    zd = nodes[i].z - nodes[i - 1].z;
                    len += (float)Math.Sqrt((xd * xd + yd * yd + zd * zd));
                }

                mangos_time = (uint)(len * 33.360f);    // 33.373f / 33.336

                sw.WriteLine("Mangostime 3D: " + mangos_time);

                mangos_time = 0;
                len = 0;

                for (int i = 1; i < points; i++)
                {
                    xd = nodes[i].x - nodes[i - 1].x;
                    yd = nodes[i].y - nodes[i - 1].y;
                    len += (float)Math.Sqrt((xd * xd + yd * yd));
                }

                mangos_time = (uint)(len * 33.360f);

                sw.WriteLine("Mangostime 2D: " + mangos_time);
                sw.WriteLine();

                sw.Flush();
                sw.Close();
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="gr2"></param>
        /// <param name="sb"></param>
        /// <param name="swe"></param>
        /// <returns></returns>
        public static bool ParseInitialSpellsOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            sb.AppendLine("Packet offset " + gr.BaseStream.Position.ToString("X2"));
            sb.AppendLine("Opcode SMSG_INITIAL_SPELLS (0x012A)");

            byte unk1 = gr2.ReadByte();

            sb.AppendLine("unk byte " + unk1);

            ushort spells_count = gr2.ReadUInt16();
            sb.AppendLine("Spells count " + spells_count);
            for (ushort i = 0; i < spells_count; i++)
            {
                ushort spellid = gr2.ReadUInt16();
                ushort slotid = gr2.ReadUInt16();
                sb.AppendLine("Spell ID " + spellid + ", slot " + slotid.ToString("X2"));
            }

            ushort cooldowns_count = gr2.ReadUInt16();
            sb.AppendLine("Cooldowns count " + cooldowns_count);
            for (ushort i = 0; i < cooldowns_count; i++)
            {
                ushort spellid = gr2.ReadUInt16();
                ushort itemid = gr2.ReadUInt16();
                ushort spellcategory = gr2.ReadUInt16();
                uint cooldown1 = gr2.ReadUInt32();
                uint cooldown2 = gr2.ReadUInt32();
                sb.AppendLine("Spell Cooldown: spell id " + spellid + ", itemid " + itemid + ", spellcategory " + spellcategory + ", cooldown1 " + cooldown1 + ", cooldown2 " + cooldown2);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="gr2"></param>
        /// <param name="sb"></param>
        /// <param name="swe"></param>
        /// <returns></returns>
        public static bool ParseAuctionListResultOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            sb.AppendLine("Packet offset ");
            sb.AppendLine(gr.BaseStream.Position.ToString("X2"));
            sb.AppendLine("Opcode SMSG_AUCTION_LIST_RESULT (0x025C)");

            uint count = gr2.ReadUInt32();

            sb.AppendLine("count ");
            sb.AppendLine(count.ToString());

            for (uint i = 0; i < count; i++)
            {
                uint auction_id = gr2.ReadUInt32();
                sb.AppendLine("auction_id " + auction_id);
                uint item_entry = gr2.ReadUInt32();
                sb.AppendLine("item_entry " + item_entry);

                uint ench1_1 = gr2.ReadUInt32();
                uint ench1_2 = gr2.ReadUInt32();
                uint ench1_3 = gr2.ReadUInt32();
                sb.AppendLine("enchant1 " + ench1_1 + " " + ench1_2 + " " + ench1_3);

                uint ench2_1 = gr2.ReadUInt32();
                uint ench2_2 = gr2.ReadUInt32();
                uint ench2_3 = gr2.ReadUInt32();
                sb.AppendLine("enchant2 " + ench2_1 + " " + ench2_2 + " " + ench2_3);

                uint socket1_1 = gr2.ReadUInt32();
                uint socket1_2 = gr2.ReadUInt32();
                uint socket1_3 = gr2.ReadUInt32();
                sb.AppendLine("socket1 " + socket1_1 + " " + socket1_2 + " " + socket1_3);

                uint socket2_1 = gr2.ReadUInt32();
                uint socket2_2 = gr2.ReadUInt32();
                uint socket2_3 = gr2.ReadUInt32();
                sb.AppendLine("socket2 " + socket2_1 + " " + socket2_2 + " " + socket2_3);

                uint socket3_1 = gr2.ReadUInt32();
                uint socket3_2 = gr2.ReadUInt32();
                uint socket3_3 = gr2.ReadUInt32();
                sb.AppendLine("socket3 " + socket3_1 + " " + socket3_2 + " " + socket3_3);

                uint bonus_1 = gr2.ReadUInt32();
                uint bonus_2 = gr2.ReadUInt32();
                uint bonus_3 = gr2.ReadUInt32();
                sb.AppendLine("bonus " + bonus_1 + " " + bonus_2 + " " + bonus_3);

                uint rand = gr2.ReadUInt32();
                sb.AppendLine("random property " + rand);

                uint unk1 = gr2.ReadUInt32();
                sb.AppendLine("unk1 " + unk1);

                uint itemcount = gr2.ReadUInt32();
                sb.AppendLine("item count " + itemcount);

                uint charges = gr2.ReadUInt32();
                sb.AppendLine("charges " + charges);
                uint unk2 = gr2.ReadUInt32();
                sb.AppendLine("unk2 " + unk2);

                if (unk2 != 0)
                    swe.WriteLine("unk2 " + unk2);

                ulong owner = gr2.ReadUInt64();
                uint startbid = gr2.ReadUInt32();
                uint outbid = gr2.ReadUInt32();
                uint buyout = gr2.ReadUInt32();
                uint time = gr2.ReadUInt32();
                sb.AppendLine("owner: " + owner + " " + startbid + " " + outbid + " " + buyout + " " + time);

                ulong bidder = gr2.ReadUInt64();
                uint bid = gr2.ReadUInt32();
                sb.AppendLine("bidder " + bidder + " " + bid);
                sb.AppendLine();
            }

            uint totalcount = gr2.ReadUInt32();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="gr2"></param>
        /// <param name="sb"></param>
        /// <param name="swe"></param>
        /// <returns></returns>
        public static bool ParsePartyMemberStatsOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            sb.AppendLine("Packet offset " + gr.BaseStream.Position.ToString("X2"));
            sb.AppendLine("Opcode SMSG_PARTY_MEMBER_STATS (0x007E)");

            byte MAX_AURAS = 56;

            ulong guid = gr2.ReadPackedGuid();
            sb.AppendLine("GUID " + guid.ToString("X16"));

            GroupUpdateFlags flags = (GroupUpdateFlags)gr2.ReadUInt32();
            sb.AppendLine("Flags " + flags);

            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_ONLINE) != 0)
            {
                GroupMemberOnlineStatus online = (GroupMemberOnlineStatus)gr2.ReadUInt16(); // flag
                sb.AppendLine("Online state " + online);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_CUR_HP) != 0)
            {
                ushort hp = gr2.ReadUInt16();
                sb.AppendLine("Cur. health " + hp);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_MAX_HP) != 0)
            {
                ushort maxhp = gr2.ReadUInt16();
                sb.AppendLine("Max health " + maxhp);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_POWER_TYPE) != 0)
            {
                Powers power = (Powers)gr2.ReadByte();
                sb.AppendLine("Power type " + power);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_CUR_POWER) != 0)
            {
                ushort curpower = gr2.ReadUInt16();
                sb.AppendLine("Cur. power " + curpower);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_MAX_POWER) != 0)
            {
                ushort maxpower = gr2.ReadUInt16();
                sb.AppendLine("Max power " + maxpower);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_LEVEL) != 0)
            {
                ushort level = gr2.ReadUInt16();
                sb.AppendLine("Level " + level);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_ZONE) != 0)
            {
                ushort zone = gr2.ReadUInt16();
                sb.AppendLine("Zone " + zone);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_POSITION) != 0)
            {
                short x = gr2.ReadInt16();
                short y = gr2.ReadInt16();
                sb.AppendLine("Position: " + x + ", " + y);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_AURAS) != 0)
            {
                ulong mask = gr2.ReadUInt64();
                sb.AppendLine("Auras mask " + mask.ToString("X16"));

                BitArray bitArr = new BitArray(BitConverter.GetBytes(mask));

                for (int i = 0; i < bitArr.Length; i++)
                {
                    if (i >= MAX_AURAS) // we can have only 56 auras
                        break;

                    if (bitArr[i])
                    {
                        ushort spellid = gr2.ReadUInt16();
                        sb.AppendLine("Aura " + i.ToString() + ": " + spellid.ToString());
                        byte unk = gr2.ReadByte();
                        sb.AppendLine("Aura unk " + i.ToString() + ": " + unk.ToString());
                    }
                }
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_PET_GUID) != 0)
            {
                ulong petguid = gr2.ReadUInt64();
                sb.AppendLine("Pet guid " + petguid.ToString("X16"));
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_PET_NAME) != 0)
            {
                string name = gr2.ReadStringNull();
                sb.AppendLine("Pet name " + name);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_PET_MODEL_ID) != 0)
            {
                ushort modelid = gr2.ReadUInt16();
                sb.AppendLine("Pet model id " + modelid);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_PET_CUR_HP) != 0)
            {
                ushort pethp = gr2.ReadUInt16();
                sb.AppendLine("Pet cur. HP " + pethp);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_PET_MAX_HP) != 0)
            {
                ushort petmaxhp = gr2.ReadUInt16();
                sb.AppendLine("Pet max HP " + petmaxhp);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_PET_POWER_TYPE) != 0)
            {
                Powers power = (Powers)gr2.ReadByte();
                sb.AppendLine("Pet power type " + power);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_PET_CUR_POWER) != 0)
            {
                ushort petpower = gr2.ReadUInt16();
                sb.AppendLine("Pet cur. power " + petpower);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_PET_MAX_POWER) != 0)
            {
                ushort petmaxpower = gr2.ReadUInt16();
                sb.AppendLine("Pet max power " + petmaxpower);
            }
            if ((flags & GroupUpdateFlags.GROUP_UPDATE_FLAG_PET_AURAS) != 0)
            {
                ulong mask = gr2.ReadUInt64();
                sb.AppendLine("Pet auras mask " + mask.ToString("X16"));

                BitArray bitArr = new BitArray(BitConverter.GetBytes(mask));
                for (int i = 0; i < bitArr.Length; i++)
                {
                    if (i >= MAX_AURAS) // we can have only 56 auras
                        break;

                    if (bitArr[i])
                    {
                        ushort spellid = gr2.ReadUInt16();
                        sb.AppendLine("Pet aura " + i.ToString() + ": " + spellid.ToString());
                        byte unk = gr2.ReadByte();
                        sb.AppendLine("Pet aura unk " + i.ToString() + ": " + unk.ToString());
                    }
                    i++;
                }
            }

            if (gr2.BaseStream.Position == gr2.BaseStream.Length)
                sb.AppendLine("parsed: ok...");
            else
                sb.AppendLine("parsed: error...");

            return true;
        }

        public static bool ParseTrainerListOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            sb.AppendLine("Packet offset " + gr.BaseStream.Position.ToString("X2"));
            sb.AppendLine("Opcode SMSG_TRAINER_LIST (0x01B1)");

            StreamWriter sw = new StreamWriter("trainer.log", true, Encoding.ASCII);

            ulong guid = gr2.ReadUInt64();
            TrainerType trainer_type = (TrainerType)gr2.ReadUInt32();
            uint spells_count = gr2.ReadUInt32();

            sw.WriteLine("Trainer {0}, type {1}, spells_count {2}", guid.ToString("X16"), trainer_type, spells_count);

            for (uint i = 0; i < spells_count; i++)
            {
                uint spellid = gr2.ReadUInt32();
                TrainerSpellState state = (TrainerSpellState)gr2.ReadByte();
                uint spellcost = gr2.ReadUInt32();
                uint unk1 = gr2.ReadUInt32();   // isProfession?
                uint unk2 = gr2.ReadUInt32();
                byte reqlevel = gr2.ReadByte();
                uint reqskill = gr2.ReadUInt32();
                uint reqskillvalue = gr2.ReadUInt32();
                uint reqspell = gr2.ReadUInt32();
                uint unk3 = gr2.ReadUInt32();
                uint unk4 = gr2.ReadUInt32();

                sw.WriteLine("Spell {0}, state {1}, cost {2}, unk1 {3}, unk2 {4}, reqlevel {5}, reqskill {6}, reqskillvalue {7}, reqspell {8}, unk3 {9} unk4 {10}", spellid, state, spellcost, unk1, unk2, reqlevel, reqskill, reqskillvalue, reqspell, unk3, unk4);
            }

            string title = gr2.ReadStringNull();
            sw.WriteLine("title {0}", title);

            sw.Flush();
            sw.Close();

            if (gr2.BaseStream.Position == gr2.BaseStream.Length)
                sb.AppendLine("parsed: ok...");
            else
                sb.AppendLine("parsed: error...");

            return true;
        }

        public static bool ParseAttackerStateUpdateOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            sb.AppendLine("Packet offset " + gr.BaseStream.Position.ToString("X2"));
            sb.AppendLine("Opcode SMSG_ATTACKERSTATEUPDATE (0x01B1)");

            StreamWriter sw = new StreamWriter("attacker_state.log", true, Encoding.ASCII);

            HitInfo hi = (HitInfo)gr2.ReadUInt32();
            ulong attacker = gr2.ReadPackedGuid();
            ulong target = gr2.ReadPackedGuid();

            uint damage = gr2.ReadUInt32();

            sw.WriteLine("HitInfo {0}", hi);
            sw.WriteLine("attacker {0}", attacker.ToString("X16"));
            sw.WriteLine("target {0}", target.ToString("X16"));
            sw.WriteLine("damage {0}", damage);

            byte count = gr2.ReadByte();
            sw.WriteLine("count {0}", count);

            for (byte i = 0; i < count; i++)
            {
                ITEM_DAMAGE_TYPE damagetype = (ITEM_DAMAGE_TYPE)gr2.ReadUInt32();
                float damage2 = gr2.ReadSingle();
                uint damage3 = gr2.ReadUInt32();
                uint adsorb = gr2.ReadUInt32();
                uint resist = gr2.ReadUInt32();

                sw.WriteLine("damagetype {0}", damagetype);
                sw.WriteLine("damage2 {0}", damage2);
                sw.WriteLine("damage3 {0}", damage3);
                sw.WriteLine("adsorb {0}", adsorb);
                sw.WriteLine("resist {0}", resist);
            }

            VictimState targetstate = (VictimState)gr2.ReadUInt32();
            uint unk1 = gr2.ReadUInt32();
            uint unk2 = gr2.ReadUInt32();
            uint blocked = gr2.ReadUInt32();

            sw.WriteLine("targetstate {0}", targetstate);
            sw.WriteLine("unk1 {0}", unk1);
            sw.WriteLine("unk2 {0}", unk2);
            sw.WriteLine("blocked {0}", blocked);
            sw.WriteLine();

            sw.Flush();
            sw.Close();

            if (gr2.BaseStream.Position == gr2.BaseStream.Length)
                sb.AppendLine("parsed: ok...");
            else
                sb.AppendLine("parsed: error...");

            return true;
        }

        public static bool ParseLoginSetTimeSpeedOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            sb.AppendLine("Packet offset " + gr.BaseStream.Position.ToString("X2"));
            sb.AppendLine("Opcode SMSG_LOGIN_SETTIMESPEED (0x0042)");

            StreamWriter sw = new StreamWriter("login_timespeed.log", true, Encoding.ASCII);

            uint time = gr2.ReadUInt32();
            float speed = gr2.ReadSingle();

            sw.WriteLine("time {0}, speed {1}", time, speed);

            sw.Flush();
            sw.Close();

            if (gr2.BaseStream.Position == gr2.BaseStream.Length)
                sb.AppendLine("parsed: ok...");
            else
                sb.AppendLine("parsed: error...");

            return true;
        }

        public static bool ParseCorpseQueryOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            if (direction == 0)  // client packet
                return false;

            StreamWriter sw = new StreamWriter("corpse_query.log", true, Encoding.ASCII);

            sw.WriteLine("Packet offset {0}", gr.BaseStream.Position.ToString("X2"));
            sw.WriteLine("Opcode MSG_CORPSE_QUERY (0x0216)");

            byte unk = gr2.ReadByte();
            sw.WriteLine("unk {0}", unk);
            if (unk > 0)
            {
                uint mapid1 = gr2.ReadUInt32();
                float x = gr2.ReadSingle();
                float y = gr2.ReadSingle();
                float z = gr2.ReadSingle();
                uint mapid2 = gr2.ReadUInt32();
                sw.WriteLine("map {0}, pos {1} {2} {3}", mapid1, x, y, z);
                sw.WriteLine("map2 {0}", mapid2);
            }

            if (gr2.BaseStream.Position == gr2.BaseStream.Length)
                sw.WriteLine("parsed: ok...");
            else
                sw.WriteLine("parsed: error...");

            sw.WriteLine();
            sw.Flush();
            sw.Close();

            return true;
        }

        public static bool ParseLoginVerifyWorldOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            // used to get current map id
            uint mapid = gr2.ReadUInt32();
            Coords4 coords = gr2.ReadCoords4();

            MapId = mapid;
            return true;
        }

        public static bool ParseSpellNonMeleeDamageLogOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            StreamWriter sw = new StreamWriter("SpellNonMeleeDamageLog.log", true, Encoding.ASCII);

            sw.WriteLine("Packet offset {0}", gr.BaseStream.Position.ToString("X2"));
            sw.WriteLine("Opcode SpellNonMeleeDamageLog (0x0250)");

            ulong target = gr2.ReadPackedGuid();
            ulong caster = gr2.ReadPackedGuid();
            sw.WriteLine("target {0}, caster {1}", target, caster);
            uint spellid = gr2.ReadUInt32();
            sw.WriteLine("spell {0}", spellid);
            uint damage = gr2.ReadUInt32();
            sw.WriteLine("damage {0}", damage);
            byte unk1 = gr2.ReadByte();
            sw.WriteLine("unk1 {0}", unk1);
            uint adsorb = gr2.ReadUInt32();
            sw.WriteLine("adsorb {0}", adsorb);
            uint resist = gr2.ReadUInt32();
            sw.WriteLine("resist {0}", resist);
            byte unk2 = gr2.ReadByte();
            sw.WriteLine("unk2 {0}", unk2);
            byte unk3 = gr2.ReadByte();
            sw.WriteLine("unk3 {0}", unk3);
            uint blocked = gr2.ReadUInt32();
            sw.WriteLine("blocked {0}", blocked);
            uint flags = gr2.ReadUInt32();
            sw.WriteLine("flags {0}", flags.ToString("X8"));
            byte unk4 = gr2.ReadByte();
            sw.WriteLine("unk4 {0}", unk4);

            if ((flags & 0x1) != 0)
            {
                float a = gr2.ReadSingle();
                float b = gr2.ReadSingle();
                sw.WriteLine("0x1: a {0}, b {1}", a, b);
            }
            if ((flags & 0x4) != 0)
            {
                float a = gr2.ReadSingle();
                float b = gr2.ReadSingle();
                sw.WriteLine("0x4: a {0}, b {1}", a, b);
            }
            if ((flags & 0x20) != 0)
            {
                float a = gr2.ReadSingle();
                float b = gr2.ReadSingle();
                float c = gr2.ReadSingle();
                float d = gr2.ReadSingle();
                float e = gr2.ReadSingle();
                float f = gr2.ReadSingle();
                sw.WriteLine("0x20: a {0}, b {1}, c {2}, d {3}, e {4}, f {5}", a, b, c, d, e, f);
            }

            if (gr2.BaseStream.Position != gr2.BaseStream.Length)
                sw.WriteLine("FUCK!: " + flags.ToString("X8"));

            sw.WriteLine();
            sw.Flush();
            sw.Close();

            return true;
        }

        public static bool ParseSpellLogExecuteOpcode(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe, byte direction)
        {
            StreamWriter sw = new StreamWriter("SpellLogExecute.log", true, Encoding.ASCII);

            sw.WriteLine("Packet offset {0}", gr.BaseStream.Position.ToString("X2"));
            sw.WriteLine("Opcode SMSG_SPELLLOGEXECUTE (0x024C)");

            ulong caster = gr2.ReadPackedGuid();
            sw.WriteLine("caster {0}", caster.ToString("X16"));
            uint spellid = gr2.ReadUInt32();
            sw.WriteLine("spellid {0}", spellid);
            uint count1 = gr2.ReadUInt32();
            sw.WriteLine("count1 {0}", count1);
            for (uint i = 0; i < count1; ++i)
            {
                uint spelleffect = gr2.ReadUInt32();
                sw.WriteLine("spelleffect {0}", spelleffect);
                uint count2 = gr2.ReadUInt32();
                sw.WriteLine("count2 {0}", count2);
                for (uint j = 0; j < count2; ++j)
                {
                    ulong guid;
                    uint unk1;
                    uint unk2;
                    float unk3;

                    switch (spelleffect)
                    {
                        case 0x08:                                  // SPELL_EFFECT_MANA_DRAIN
                            guid = gr2.ReadPackedGuid();
                            unk1 = gr2.ReadUInt32();
                            unk2 = gr2.ReadUInt32();
                            unk3 = gr2.ReadSingle();
                            sw.WriteLine("0x08: {0} {1} {2} {3}", guid.ToString("X16"), unk1, unk2, unk3);
                            break;
                        case 0x13:                                  // SPELL_EFFECT_ADD_EXTRA_ATTACKS
                            guid = gr2.ReadPackedGuid();
                            unk1 = gr2.ReadUInt32();
                            sw.WriteLine("0x13: {0} {1}", guid.ToString("X16"), unk1);
                            break;
                        case 0x44:                                  // SPELL_EFFECT_INTERRUPT_CAST
                            guid = gr2.ReadPackedGuid();
                            unk1 = gr2.ReadUInt32();
                            sw.WriteLine("0x44: {0} {1}", guid.ToString("X16"), unk1);
                            break;
                        case 0x6F:                                  // SPELL_EFFECT_DURABILITY_DAMAGE
                            guid = gr2.ReadPackedGuid();
                            unk1 = gr2.ReadUInt32();
                            unk2 = gr2.ReadUInt32();
                            sw.WriteLine("0x6F: {0} {1} {2}", guid.ToString("X16"), unk1, unk2);
                            break;
                        case 0x21:                                  // SPELL_EFFECT_OPEN_LOCK
                        case 0x3B:                                  // SPELL_EFFECT_OPEN_LOCK_ITEM
                            guid = gr2.ReadPackedGuid();
                            sw.WriteLine("0x21,0x3B: {0}", guid.ToString("X16"));
                            break;
                        case 0x18:                                  // SPELL_EFFECT_CREATE_ITEM
                            unk1 = gr2.ReadUInt32();
                            sw.WriteLine("0x18: {0}", unk1);
                            break;
                        case 0x1C:                                  // SPELL_EFFECT_SUMMON
                        case 0x29:                                  // SPELL_EFFECT_SUMMON_WILD
                        case 0x2A:                                  // SPELL_EFFECT_SUMMON_GUARDIAN
                        case 0x32:                                  // SPELL_EFFECT_TRANS_DOOR
                        case 0x38:                                  // SPELL_EFFECT_SUMMON_PET
                        case 0x49:                                  // SPELL_EFFECT_SUMMON_POSSESSED
                        case 0x4A:                                  // SPELL_EFFECT_SUMMON_TOTEM
                        case 0x4C:                                  // SPELL_EFFECT_SUMMON_OBJECT_WILD
                        case 0x51:                                  // SPELL_EFFECT_CREATE_HOUSE
                        case 0x53:                                  // SPELL_EFFECT_DUEL
                        case 0x57:                                  // SPELL_EFFECT_SUMMON_TOTEM_SLOT1
                        case 0x58:                                  // SPELL_EFFECT_SUMMON_TOTEM_SLOT2
                        case 0x59:                                  // SPELL_EFFECT_SUMMON_TOTEM_SLOT3
                        case 0x5A:                                  // SPELL_EFFECT_SUMMON_TOTEM_SLOT4
                        case 0x5D:                                  // SPELL_EFFECT_SUMMON_PHANTASM
                        case 0x61:                                  // SPELL_EFFECT_SUMMON_CRITTER
                        case 0x68:                                  // SPELL_EFFECT_SUMMON_OBJECT_SLOT1
                        case 0x69:                                  // SPELL_EFFECT_SUMMON_OBJECT_SLOT2
                        case 0x6A:                                  // SPELL_EFFECT_SUMMON_OBJECT_SLOT3
                        case 0x6B:                                  // SPELL_EFFECT_SUMMON_OBJECT_SLOT4
                        case 0x70:                                  // SPELL_EFFECT_SUMMON_DEMON
                        case 0x96:                                  // SPELL_EFFECT_150
                            guid = gr2.ReadPackedGuid();
                            sw.WriteLine("summon: {0}", guid.ToString("X16"));
                            break;
                        case 0x65:                                  // SPELL_EFFECT_FEED_PET
                            unk1 = gr2.ReadUInt32();
                            sw.WriteLine("0x65: {0}", unk1);
                            break;
                        case 0x66:                                  // SPELL_EFFECT_DISMISS_PET
                            guid = gr2.ReadPackedGuid();
                            sw.WriteLine("0x66: {0}", guid.ToString("X16"));
                            break;
                        default:
                            sw.WriteLine("unknown spell effect {0}", spelleffect);
                            break;
                    }
                }
            }

            if (gr2.BaseStream.Position != gr2.BaseStream.Length)
                sw.WriteLine("FUCK!");

            sw.WriteLine();
            sw.Flush();
            sw.Close();

            return true;
        }
    }
}
