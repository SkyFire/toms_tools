using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace a9_parser
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Update mask bit array.
        /// </summary>
        public BitArr Mask;
        uint packet = 1;

        /// <summary>
        /// Constructor for application form.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ifn = Directory.GetCurrentDirectory() + "\\" + "A9.dump";

            if (!File.Exists(ifn))
            {
                MessageBox.Show("File " + ifn + " not found!");
                return;
            }

            GenericReader gr = new GenericReader(ifn, Encoding.ASCII);

            string error_log = "errors.txt";
            StreamWriter swe = new StreamWriter(error_log);

            string database_log = "data.txt";
            StreamWriter data = new StreamWriter(database_log);

            string ofn = "A9_out.txt";
            StreamWriter sw = new StreamWriter(ofn);

            sw.AutoFlush = true;
            swe.AutoFlush = true;
            data.AutoFlush = true;

            // A9 packet structure:
            // uint count (4 bytes)
            // byte 0 (unk, can be 1, 2 also...) (1 byte)
            // for(uint i = 0; i < count; i++)
            // {
            //     byte updatetype (all below for UPDATETYPE_VALUES) (1 byte)
            //     ulong guid(packed) (2-9 bytes)
            //     byte count_of_blocks (1 byte)
            //     uint*count_of_blocks bitmask array (4*count_of_blocks bytes)
            //     for each non zero bit in bitmask exist block of data (uint/float) (4 bytes)
            // }

            //MessageBox.Show(br.BaseStream.Position.ToString() + " " +  br.BaseStream.Length.ToString());

            try
            {
                while (gr.PeekChar() >= 0)
                {
                    uint result = ParsePacket(gr, sw, swe, data);
                    if (result == 0 || result == 1)
                        packet++;
                }
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }

            sw.Close();
            swe.Close();
            data.Close();

            MessageBox.Show("Done!", "A9 parser", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
            //MessageBox.Show(br.BaseStream.Position.ToString() + " " + br.BaseStream.Length.ToString());

            gr.Close();
        }

        private uint ParsePacket(GenericReader gr, StreamWriter sw, StreamWriter swe, StreamWriter data)
        {
            StringBuilder sb = new StringBuilder();

            //MessageBox.Show(br.BaseStream.Position.ToString() + " " + br.BaseStream.Length.ToString());

            uint opcode = gr.ReadUInt16();
            if (opcode != 0x00A9)
            {
                return 2;
            }

            sb.AppendLine("Packet offset " + (gr.BaseStream.Position - 2).ToString("X2"));

            sb.AppendLine("Packet number: " + packet);

            sb.AppendLine("Opcode: " + opcode.ToString("X4"));

            uint count = gr.ReadUInt32();
            sb.AppendLine("Object count: " + count);

            uint unk = gr.ReadByte();
            sb.AppendLine("Unk: " + unk);

            for (uint i = 1; i < count+1; i++)
            {
                sb.AppendLine("Update block for object " + i + ":");
                sb.AppendLine("Block offset " + gr.BaseStream.Position.ToString("X2"));

                if (!ParseBlock(gr, sb, swe, data))
                {
                    sw.WriteLine(sb.ToString());
                    return 1;
                }
            }
            sw.WriteLine(sb.ToString());
            return 0;
        }

        private bool ParseBlock(GenericReader gr, StringBuilder sb, StreamWriter swe, StreamWriter data)
        {
            UpdateTypes updatetype = (UpdateTypes)gr.ReadByte();
            sb.AppendLine("Updatetype: " + updatetype);

            if (updatetype < UpdateTypes.UPDATETYPE_VALUES || updatetype > UpdateTypes.UPDATETYPE_NEAR_OBJECTS)
            {
                long pos = gr.BaseStream.Position;
                swe.WriteLine("wrong updatetype at position " + pos.ToString("X2"));

                // we there only if we read packet wrong way
                swe.WriteLine("Updatetype " + updatetype + " is not supported");
                return false;
            }

            if (updatetype == UpdateTypes.UPDATETYPE_VALUES)
            {
                ulong guid = gr.ReadPackedGuid();
                sb.AppendLine("Object guid: " + guid.ToString("X16"));

                if (guid == 0)
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("wrong guid at position " + pos.ToString("X2"));

                    // we there only if we read packet wrong way
                    swe.WriteLine("Updatetype " + updatetype + " can't be with NULL guid");
                    return false;
                }

                // object type detection:
                ObjectTypes objecttype;
                if (guid.ToString("X16").Substring(0, 8) == "40000000")
                    objecttype = ObjectTypes.TYPEID_ITEM;
                else if (guid.ToString("X16").Substring(0, 8) == "00000000")
                    objecttype = ObjectTypes.TYPEID_PLAYER;
                else
                    objecttype = ObjectTypes.TYPEID_UNIT;

                if (!ParseValuesUpdateBlock(gr, sb, swe, data, objecttype, updatetype))
                    return false;

                return true;
            }

            if (updatetype == UpdateTypes.UPDATETYPE_MOVEMENT)
            {
                ulong guid = gr.ReadPackedGuid();
                sb.AppendLine("Object guid: " + guid.ToString("X2"));

                if (!ParseMovementUpdateBlock(gr, sb, swe, data, ObjectTypes.TYPEID_UNIT))
                    return false;

                return true;
            }

            if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
            {
                ulong guid = gr.ReadPackedGuid();
                sb.AppendLine("Object guid: " + guid.ToString("X2"));

                ObjectTypes objectTypeId = (ObjectTypes)gr.ReadByte();
                sb.AppendLine("objectTypeId " + objectTypeId);

                switch (objectTypeId)
                {
                    case ObjectTypes.TYPEID_OBJECT:
                        swe.WriteLine("Unhandled object type " + objectTypeId);
                        break;
                    case ObjectTypes.TYPEID_ITEM:
                    case ObjectTypes.TYPEID_CONTAINER:
                    case ObjectTypes.TYPEID_UNIT:
                    case ObjectTypes.TYPEID_PLAYER:
                    case ObjectTypes.TYPEID_GAMEOBJECT:
                    case ObjectTypes.TYPEID_DYNAMICOBJECT:
                    case ObjectTypes.TYPEID_CORPSE:
                        if (!ParseMovementUpdateBlock(gr, sb, swe, data, objectTypeId))
                            return false;
                        if (!ParseValuesUpdateBlock(gr, sb, swe, data, objectTypeId, updatetype))
                            return false;
                        break;
                    case ObjectTypes.TYPEID_AIGROUP:
                        swe.WriteLine("Unhandled object type " + objectTypeId);
                        break;
                    case ObjectTypes.TYPEID_AREATRIGGER:
                        swe.WriteLine("Unhandled object type " + objectTypeId);
                        break;
                    default:
                        swe.WriteLine("Unknown object type " + objectTypeId);
                        return false;
                }
                return true;
            }

            if (updatetype == UpdateTypes.UPDATETYPE_OUT_OF_RANGE_OBJECTS || updatetype == UpdateTypes.UPDATETYPE_NEAR_OBJECTS)
            {
                uint objects_count = gr.ReadUInt32();

                if (objects_count > 1000) // we read packet wrong way
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("Too many " + updatetype + " objects");
                    return false;
                }

                sb.AppendLine("guids_count " + objects_count);

                for(uint i = 0; i < objects_count; i++)
                    sb.AppendLine("Guid" + i + ": " + gr.ReadPackedGuid());
                return true;
            }

            return true;
        }

        private bool ParseValuesUpdateBlock(GenericReader gr, StringBuilder sb, StreamWriter swe, StreamWriter data, ObjectTypes objectTypeId, UpdateTypes updatetype)
        {
            // may be we can reduce size of code there...

            sb.AppendLine("=== values_update_block_start ===");

            byte blocks_count = gr.ReadByte(); // count of update blocks (4 bytes for each update block)
            sb.AppendLine("Bit mask blocks count: " + blocks_count);

            uint[] updatemask = new uint[blocks_count]; // create array of update blocks
            for (int j = 0; j < blocks_count; j++)
                updatemask[j] = gr.ReadUInt32(); // populate array of update blocks with data

            Mask = (BitArr)updatemask; // convert array of update blocks to bitmask array

            int reallength = Mask.RealLength; // bitmask size (bits)

            // item/container values update block
            if (objectTypeId == ObjectTypes.TYPEID_ITEM || objectTypeId == ObjectTypes.TYPEID_CONTAINER)
            {
                if (reallength > 160) // 5*32, ??
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing ITEM values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.ITEM_END)
                        break;

                    if (Mask[index])
                    {
                        switch (UpdateFields.item_updatefields[index].type)
                        {
                            case 1:
                                sb.AppendLine(UpdateFields.item_updatefields[index].name + " (" + index + "): " + gr.ReadSingle().ToString().Replace(",", "."));
                                break;
                            default:
                                sb.AppendLine(UpdateFields.item_updatefields[index].name + " (" + index + "): " + gr.ReadUInt32());
                                break;
                        }
                    }
                }
            }

            // unit values update block
            if (objectTypeId == ObjectTypes.TYPEID_UNIT)
            {
                if (reallength > 256) // 32*8 (for units bitmask = 8)
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing UNIT values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.UNIT_END)
                        break;

                    if (Mask[index])
                    {
                        switch (UpdateFields.unit_updatefields[index].type)
                        {
                            case 1:
                                string val1 = gr.ReadSingle().ToString().Replace(",", ".");
                                if(updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
                                    data.WriteLine(UpdateFields.unit_updatefields[index].name + " (" + index + "): " + val1);
                                sb.AppendLine(UpdateFields.unit_updatefields[index].name + " (" + index + "): " + val1);
                                break;
                            default:
                                uint val2 = gr.ReadUInt32();
                                if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
                                    data.WriteLine(UpdateFields.unit_updatefields[index].name + " (" + index + "): " + val2);
                                sb.AppendLine(UpdateFields.unit_updatefields[index].name + " (" + index + "): " + val2);
                                break;
                        }
                    }
                }
            }

            // player values update block
            if (objectTypeId == ObjectTypes.TYPEID_PLAYER)
            {
                if (reallength > 1440) // 32*45 (for player bitmask = 45)
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing PLAYER values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.PLAYER_END)
                        break;

                    if (Mask[index])
                    {
                        switch (UpdateFields.unit_updatefields[index].type)
                        {
                            case 1:
                                string val1 = gr.ReadSingle().ToString().Replace(",", ".");
                                sb.AppendLine(UpdateFields.unit_updatefields[index].name + " (" + index + "): " + val1);
                                break;
                            default:
                                uint val2 = gr.ReadUInt32();
                                sb.AppendLine(UpdateFields.unit_updatefields[index].name + " (" + index + "): " + val2);
                                break;
                        }
                    }
                }
            }

            // gameobject values update block
            if (objectTypeId == ObjectTypes.TYPEID_GAMEOBJECT)
            {
                if (reallength > 32) // 1*32
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing GO values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.GO_END)
                        break;

                    if (Mask[index])
                    {
                        switch (UpdateFields.go_updatefields[index].type)
                        {
                            case 1:
                                string val1 = gr.ReadSingle().ToString().Replace(",", ".");
                                if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
                                    data.WriteLine(UpdateFields.go_updatefields[index].name + " (" + index + "): " + val1);
                                sb.AppendLine(UpdateFields.go_updatefields[index].name + " (" + index + "): " + val1);
                                break;
                            default:
                                uint val2 = gr.ReadUInt32();
                                if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
                                    data.WriteLine(UpdateFields.go_updatefields[index].name + " (" + index + "): " + val2);
                                sb.AppendLine(UpdateFields.go_updatefields[index].name + " (" + index + "): " + val2);
                                break;
                        }
                    }
                }
            }

            // dynamicobject values update block
            if (objectTypeId == ObjectTypes.TYPEID_DYNAMICOBJECT)
            {
                if (reallength > 32) // 1*32
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing DO values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.DO_END)
                        break;

                    if (Mask[index])
                    {
                        switch (UpdateFields.do_updatefields[index].type)
                        {
                            case 1:
                                sb.AppendLine(UpdateFields.do_updatefields[index].name + " (" + index + "): " + gr.ReadSingle().ToString().Replace(",", "."));
                                break;
                            default:
                                sb.AppendLine(UpdateFields.do_updatefields[index].name + " (" + index + "): " + gr.ReadUInt32());
                                break;
                        }
                    }
                }
            }

            // corpse values update block
            if (objectTypeId == ObjectTypes.TYPEID_CORPSE)
            {
                if (reallength > 64) // 2*32
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing CORPSE values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.CORPSE_END)
                        break;

                    if (Mask[index])
                    {
                        switch (UpdateFields.corpse_updatefields[index].type)
                        {
                            case 1:
                                sb.AppendLine(UpdateFields.corpse_updatefields[index].name + " (" + index + "): " + gr.ReadSingle().ToString().Replace(",", "."));
                                break;
                            default:
                                sb.AppendLine(UpdateFields.corpse_updatefields[index].name + " (" + index + "): " + gr.ReadUInt32());
                                break;
                        }
                    }
                }
            }

            //swe.WriteLine("ok...");
            sb.AppendLine("=== values_update_block_end ===");
            return true;
        }

        private bool ParseMovementUpdateBlock(GenericReader gr, StringBuilder sb, StreamWriter swe, StreamWriter data, ObjectTypes objectTypeId)
        {
            Coords4 coords;
            // need figure out flags2, check flags, because we can't read packet without it...
            // flags2:

            // 0x1 - not affect data
            // 0x2 - need check
            // 0x4
            // 0x8
            // 0x10 - need check
            // 0x20 - not affect data
            // 0x100
            // 0x800
            // 0x2000 - need check
            // 0x4000
            // 0x200000
            // 0x8000000 ?
            // 0x10000000
            // 0x20000000

            sb.AppendLine("=== movement_update_block_start ===");
            uint flags2 = 0;

            UpdateFlags flags = (UpdateFlags)gr.ReadByte();
            sb.AppendLine("flags " + flags.ToString("X"));

            if ((UpdateFlags.UPDATEFLAG_LIVING & flags) != 0) // 0x20
            {
                flags2 = gr.ReadUInt32();
                sb.AppendLine("flags2 " + flags2.ToString("X8"));

                uint time = gr.ReadUInt32();
                sb.AppendLine("time " + time);
            }

            if ((UpdateFlags.UPDATEFLAG_HASPOSITION & flags) != 0) // 0x40
            {
                if ((UpdateFlags.UPDATEFLAG_TRANSPORT & flags) != 0) // 0x02
                {
                    coords = gr.ReadCoords4();
                    sb.AppendLine("coords " + coords.GetCoordsAsString());
                }
                else // strange, we read the same data :)
                {
                    coords = gr.ReadCoords4();
                    sb.AppendLine("coords " + coords.GetCoordsAsString());
                }

                if (objectTypeId == ObjectTypes.TYPEID_UNIT || objectTypeId == ObjectTypes.TYPEID_GAMEOBJECT)
                {
                    data.WriteLine();
                    data.WriteLine(objectTypeId + ": " + coords.GetCoordsAsString());
                }

                if ((flags2 & 0x0200) != 0) // transport
                {
                    ulong t_guid = gr.ReadUInt64();
                    sb.Append("t_guid " + t_guid.ToString("X2") + ", ");

                    Coords4 transport = gr.ReadCoords4();
                    sb.AppendLine("t_coords " + transport.GetCoordsAsString());

                    uint unk1 = gr.ReadUInt32(); // unk, 2.0.6 == 0x11 or random
                    sb.AppendLine("unk1 " + unk1);
                }
            }

            if ((UpdateFlags.UPDATEFLAG_LIVING & flags) != 0) // 0x20
            {
                uint unk1 = gr.ReadUInt32();
                sb.AppendLine("unk1 " + unk1);

                if ((flags2 & 0x2000) != 0) // <---
                {
                    // looks like orientation/coords/speed
                    float unk2 = gr.ReadSingle();
                    sb.AppendLine("unk2 " + unk2);
                    float unk3 = gr.ReadSingle();
                    sb.AppendLine("unk3 " + unk3);
                    float unk4 = gr.ReadSingle();
                    sb.AppendLine("unk4 " + unk4);
                    float unk5 = gr.ReadSingle();
                    sb.AppendLine("unk5 " + unk5);
                }

                float ws = gr.ReadSingle();
                sb.AppendLine("Walk speed " + ws);
                float rs = gr.ReadSingle();
                sb.AppendLine("Run speed " + rs);
                float sbs = gr.ReadSingle();
                sb.AppendLine("Swimback speed " + sbs);
                float ss = gr.ReadSingle();
                sb.AppendLine("Swim speed " + ss);
                float wbs = gr.ReadSingle();
                sb.AppendLine("Walkback speed " + wbs);
                float fs = gr.ReadSingle();
                sb.AppendLine("Fly speed " + fs);
                float fbs = gr.ReadSingle();
                sb.AppendLine("Flyback speed " + fbs);
                float ts = gr.ReadSingle();
                sb.AppendLine("Turn speed " + ts); // pi = 3.14
            }

            // after 2.0.10 released, I can't figure out what is flags3 and when it used...
            uint flags3 = 0;
            if ((UpdateFlags.UPDATEFLAG_ALL & flags) != 0) // 0x10
            {
                flags3 = gr.ReadUInt32(); // looks like flags (0x0, 0x1, 0x100, 0x20000, 0x40000)...
                sb.AppendLine("flags3 " + flags3.ToString("X2"));
            }
/*
            // still used, but can't figure out when...
            if ((flags2 & 0x8000000) == 0 && objectTypeId == ObjectTypes.TYPEID_UNIT)
            {
                if ((flags3 & 0x40000) != 0)
                {
                    uint f3_3 = br.ReadUInt32();
                    sb.AppendLine("flags3_unk_value3 " + f3_3);
                }

                if ((flags3 & 0x20000) != 0)
                {
                    ulong g3 = br.ReadUInt64();
                    sb.AppendLine("flags3_guid " + g3); // ????
                }
            }
*/
            if ((UpdateFlags.UPDATEFLAG_HIGHGUID & flags) != 0) // 0x08
            {
                uint guid_high = gr.ReadUInt32(); // 2.0.10 - it's not high guid anymore
                sb.AppendLine("guid_high " + guid_high);
            }

            if ((UpdateFlags.UPDATEFLAG_FULLGUID & flags) != 0) // 0x04
            //if ((UpdateFlags.UPDATEFLAG_FULLGUID & flags) != 0 && (flags3 & 0x20000) == 0) // 0x04
            //if ((UpdateFlags.UPDATEFLAG_FULLGUID & flags) != 0 && (flags3 & 0x40000) == 0) // 0x04
            {
                //long pos = br.BaseStream.Position;
                //swe.WriteLine("flags & 0x4 at position " + pos.ToString("X2"));

                ulong guid2 = gr.ReadPackedGuid(); // looks like guid, but what guid?
                sb.AppendLine("unk guid " + guid2);
            }

            if ((UpdateFlags.UPDATEFLAG_TRANSPORT & flags) != 0) // 0x02
            {
                uint time = gr.ReadUInt32();
                sb.AppendLine("t_time " + time);
            }

            if ((flags2 & 0x8000000) != 0) // splines
            {
                uint t1 = gr.ReadUInt32();
                sb.AppendLine("t1 " + t1);

                uint t2 = gr.ReadUInt32();
                sb.AppendLine("t2 " + t2);

                //outdated 2.0.10
                //uint t3 = br.ReadUInt32();
                //sb.AppendLine("t3 " + t3);

                uint coords_count = gr.ReadUInt32();
                sb.AppendLine("coords_count " + coords_count);

                //if (coords_count > 1000)
                //{
                //    coords_count = br.ReadUInt32();
                //    sb.AppendLine("second attempt to get correct coords count, now " + coords_count);
                //}

                //if (coords_count > 1000)
                //{
                //    coords_count = br.ReadUInt32();
                //    sb.AppendLine("third attempt to get correct coords count, now " + coords_count);
                //}

                if (coords_count > 1000) // prevent overflow in case wrong packet parsing :)
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing movement update block, flags: " + flags.ToString("X") + ", flags2: " + flags2.ToString("X") + ", flags3: " + flags3.ToString("X") + ", objecttype " + objectTypeId);
                    return false;
                }

                if (coords_count > 0)
                {
                    for (uint i = 0; i < coords_count; i++)
                    {
                        Coords3 v = gr.ReadCoords3();
                        sb.AppendLine("coord" + i + ": " + v.GetCoords());
                    }
                }

                Coords3 end = gr.ReadCoords3();
                sb.AppendLine("end: " + end.GetCoords());

                uint t8 = gr.ReadUInt32();
                sb.AppendLine("t8 " + t8);

                // added in 2.0.10 (really?)
                uint t9 = gr.ReadUInt32();
                sb.AppendLine("t9 " + t9);
            }

            sb.AppendLine("=== movement_update_block_end ===");
            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateFields.FillArrays();
        }
    }
}
