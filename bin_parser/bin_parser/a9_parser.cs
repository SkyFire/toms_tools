using System;
using System.Collections;
using System.IO;
using System.Text;
using bin_parser;
using Defines;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using UpdateFields;
using WoWObjects;
using WoWReader;

namespace A9parser
{
    /// <summary>
    /// This class is used for parsing A9 update packets.
    /// </summary>
    public class A9
    {
        /// <summary>
        /// Update mask bit array.
        /// </summary>
        public static BitArray Mask;

        public static GenericReader Decompress(GenericReader gr)
        {
            int uncompressedLength = gr.ReadInt32();
            byte[] input = gr.ReadBytes((int)gr.Remaining);
            byte[] output = new byte[uncompressedLength];
            gr.Close();
            InflaterInputStream istream = new InflaterInputStream(new MemoryStream(input));
            int offset = 0;
            while (true)
            {
                int size = istream.Read(output, offset, uncompressedLength);
                if (size == uncompressedLength)
                    break;
                offset += size;
                uncompressedLength -= size;
            }
            return new GenericReader(new MemoryStream(output));
        }

        public static void Decompress(ref GenericReader gr)
        {
            int uncompressedLength = gr.ReadInt32();
            byte[] output = new byte[gr.ReadInt32()];
            byte[] temp = gr.ReadBytes((int)gr.Remaining);
            gr.Close();
            Stream s = new InflaterInputStream(new MemoryStream(temp));
            int offset = 0;
            while (true)
            {
                int size = s.Read(output, offset, uncompressedLength);
                if (size == uncompressedLength) break;
                offset += size;
                uncompressedLength -= size;
            }
            gr = new GenericReader(new MemoryStream(output));
            //gr.BaseStream.Position = 0;
        }

        public static void ParseUpdatePacket(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe)
        {
            string database_log = "data.txt";
            StreamWriter data2 = new StreamWriter(database_log, true);
            data2.AutoFlush = true;

            sb.AppendLine("Packet offset " + (gr.BaseStream.Position - 2).ToString("X2"));

            //sb.AppendLine("Packet number: " + packet);

            uint count = gr2.ReadUInt32();
            sb.AppendLine("Object count: " + count);

            uint unk = gr2.ReadByte();
            sb.AppendLine("HasTransport: " + unk);

            for (uint i = 1; i < count + 1; i++)
            {
                sb.AppendLine("Update block for object " + i + ":");
                sb.AppendLine("Block offset " + gr.BaseStream.Position.ToString("X2"));

                if (!ParseBlock(gr2, sb, swe, data2))
                    break;
            }

            if (gr2.BaseStream.Position == gr2.BaseStream.Length)
                sb.AppendLine("packet parse: OK...");
            else
                sb.AppendLine("packet parse: ERROR!");

            data2.Flush();
            data2.Close();
        }

        private static bool ParseBlock(GenericReader gr, StringBuilder sb, StreamWriter swe, StreamWriter data)
        {
            ulong guid = 0;

            UpdateTypes updatetype = (UpdateTypes)gr.ReadByte();
            sb.AppendLine("Updatetype: " + updatetype);

            // check if updatetype is valid
            if (updatetype < UpdateTypes.UPDATETYPE_VALUES || updatetype > UpdateTypes.UPDATETYPE_NEAR_OBJECTS)
            {
                long pos = gr.BaseStream.Position;
                swe.WriteLine("wrong updatetype at position {0}", pos.ToString("X16"));

                // we there only if we read packet wrong way
                swe.WriteLine("Updatetype {0} is not supported", updatetype);
                return false;
            }

            switch (updatetype)
            {
                case UpdateTypes.UPDATETYPE_VALUES:
                    guid = gr.ReadPackedGuid();
                    sb.AppendLine("Object guid: " + guid.ToString("X16"));

                    if (guid == 0)
                    {
                        long pos = gr.BaseStream.Position;
                        swe.WriteLine("wrong guid at position {0}", pos.ToString("X2"));

                        // we there only if we read packet wrong way
                        swe.WriteLine("Updatetype {0} can't be with NULL guid", updatetype);
                        return false;
                    }

                    ObjectTypes objecttype = ObjectTypes.TYPEID_OBJECT; // 0
                    if (Binparser.m_objects.ContainsKey(guid))
                        objecttype = Binparser.m_objects[guid];
                    else    // try old method...
                    {
                        // object type auto detection:
                        // F00C0000, F02B0000 - gameobjects
                        // F00CXXXX, F02BXXXX - units (XXXX == 0000 for pets etc...)
                        // F02B00000000XXXX - corpses
                        // F02B00000000XXXX - dynamicobjects
                        if (guid.ToString("X16").Substring(0, 8).Equals("40000000"))
                            objecttype = ObjectTypes.TYPEID_ITEM;
                        else if (guid.ToString("X16").Substring(0, 8).Equals("00000000"))
                            objecttype = ObjectTypes.TYPEID_PLAYER;
                        else
                            objecttype = ObjectTypes.TYPEID_UNIT; // also can be go, do, corpse

                        swe.WriteLine("problem with objecttypeid detection for UPDATETYPE_VALUES");
                    }

                    sb.AppendLine("objectTypeId " + objecttype);

                    if (!ParseValuesUpdateBlock(gr, sb, swe, data, objecttype, updatetype, null))
                        return false;

                    return true;
                case UpdateTypes.UPDATETYPE_MOVEMENT:
                    guid = gr.ReadPackedGuid();
                    sb.AppendLine("Object guid: " + guid.ToString("X16"));

                    if (!ParseMovementUpdateBlock(gr, sb, swe, data, ObjectTypes.TYPEID_UNIT, null))
                        return false;

                    return true;
                case UpdateTypes.UPDATETYPE_CREATE_OBJECT:
                case UpdateTypes.UPDATETYPE_CREATE_OBJECT2:
                    guid = gr.ReadPackedGuid();
                    sb.AppendLine("Object guid: " + guid.ToString("X16"));

                    ObjectTypes objectTypeId = (ObjectTypes)gr.ReadByte();
                    sb.AppendLine("objectTypeId " + objectTypeId);

                    // check object existance and remove it if needed...
                    //if (Binparser.m_objects.ContainsKey(guid))
                    //    Binparser.m_objects.Remove(guid);

                    // now we can add this object to Dictionary to get correct object type later...
                    //Binparser.m_objects.Add(guid, objectTypeId);

                    WoWObject obj = new WoWObject(0, objectTypeId);

                    // add new object only if we not have it already
                    if (!Binparser.m_objects.ContainsKey(guid))
                    {
                        obj.IsNew = true;
                        Binparser.m_objects.Add(guid, objectTypeId);
                    }

                    switch (objectTypeId)
                    {
                        case ObjectTypes.TYPEID_OBJECT:
                        case ObjectTypes.TYPEID_AIGROUP:
                        case ObjectTypes.TYPEID_AREATRIGGER:
                            swe.WriteLine("Unhandled object type {0}", objectTypeId);
                            return false;
                        case ObjectTypes.TYPEID_ITEM:
                        case ObjectTypes.TYPEID_CONTAINER:
                        case ObjectTypes.TYPEID_UNIT:
                        case ObjectTypes.TYPEID_PLAYER:
                        case ObjectTypes.TYPEID_GAMEOBJECT:
                        case ObjectTypes.TYPEID_DYNAMICOBJECT:
                        case ObjectTypes.TYPEID_CORPSE:
                            if (!ParseMovementUpdateBlock(gr, sb, swe, data, objectTypeId, obj))
                                return false;
                            if (!ParseValuesUpdateBlock(gr, sb, swe, data, objectTypeId, updatetype, obj))
                                return false;
                            return true;
                        default:
                            swe.WriteLine("Unknown object type {0}", objectTypeId);
                            return false;
                    }
                case UpdateTypes.UPDATETYPE_OUT_OF_RANGE_OBJECTS:
                case UpdateTypes.UPDATETYPE_NEAR_OBJECTS:
                    uint objects_count = gr.ReadUInt32();

                    if (objects_count > 1000) // we read packet wrong way
                    {
                        long pos = gr.BaseStream.Position;
                        swe.WriteLine("error position {0}", pos.ToString("X2"));

                        swe.WriteLine("Too many {0} objects", updatetype);
                        return false;
                    }

                    sb.AppendLine("guids_count " + objects_count);

                    for (uint i = 0; i < objects_count; i++)
                        sb.AppendLine("Guid" + i + ": " + gr.ReadPackedGuid().ToString("X16"));
                    return true;
                default:
                    swe.WriteLine("Unknown updatetype {0}", updatetype);
                    return false;
            }
        }

        private static bool ParseValuesUpdateBlock(GenericReader gr, StringBuilder sb, StreamWriter swe, StreamWriter data, ObjectTypes objectTypeId, UpdateTypes updatetype, WoWObject obj)
        {
            sb.AppendLine("=== values_update_block_start ===");

            byte blocks_count = gr.ReadByte(); // count of update blocks (4 bytes for each update block)
            sb.AppendLine("Bit mask blocks count: " + blocks_count);

            int[] updatemask = new int[blocks_count]; // create array of update blocks
            for (int j = 0; j < blocks_count; j++)
                updatemask[j] = gr.ReadInt32(); // populate array of update blocks with data

            Mask = new BitArray(updatemask);

            int reallength = Mask.Count; // bitmask size (bits)

            int bitmask_max_size = 0;
            uint values_end = 0;

            switch (objectTypeId)
            {
                case ObjectTypes.TYPEID_ITEM:
                    bitmask_max_size = 64;
                    values_end = UpdateFieldsLoader.ITEM_END;
                    break;
                case ObjectTypes.TYPEID_CONTAINER:
                    bitmask_max_size = 160;
                    values_end = UpdateFieldsLoader.CONTAINER_END;
                    break;
                case ObjectTypes.TYPEID_UNIT:
                    bitmask_max_size = 256;
                    values_end = UpdateFieldsLoader.UNIT_END;
                    break;
                case ObjectTypes.TYPEID_PLAYER:
                    bitmask_max_size = 1536; // 2.3.2 - 1472
                    values_end = UpdateFieldsLoader.PLAYER_END;
                    break;
                case ObjectTypes.TYPEID_GAMEOBJECT:
                    bitmask_max_size = 32;
                    values_end = UpdateFieldsLoader.GO_END;
                    break;
                case ObjectTypes.TYPEID_DYNAMICOBJECT:
                    bitmask_max_size = 32;
                    values_end = UpdateFieldsLoader.DO_END;
                    break;
                case ObjectTypes.TYPEID_CORPSE:
                    bitmask_max_size = 64;
                    values_end = UpdateFieldsLoader.CORPSE_END;
                    break;
            }

            if (reallength > bitmask_max_size)
            {
                long pos = gr.BaseStream.Position;
                swe.WriteLine("error position {0}", pos.ToString("X2"));

                swe.WriteLine("error while parsing {0} values update block, count {1}", objectTypeId, reallength);
                return false;
            }

            for (int index = 0; index < reallength; index++)
            {
                if (index > values_end)
                    break;

                if (Mask[index])
                {
                    UpdateField uf = new UpdateField();
                    switch (objectTypeId)
                    {
                        case ObjectTypes.TYPEID_ITEM:
                        case ObjectTypes.TYPEID_CONTAINER:
                            uf = UpdateFieldsLoader.item_uf[index];
                            break;
                        case ObjectTypes.TYPEID_UNIT:
                        case ObjectTypes.TYPEID_PLAYER:
                            uf = UpdateFieldsLoader.unit_uf[index];
                            break;
                        case ObjectTypes.TYPEID_GAMEOBJECT:
                            uf = UpdateFieldsLoader.go_uf[index];
                            break;
                        case ObjectTypes.TYPEID_DYNAMICOBJECT:
                            uf = UpdateFieldsLoader.do_uf[index];
                            break;
                        case ObjectTypes.TYPEID_CORPSE:
                            uf = UpdateFieldsLoader.corpse_uf[index];
                            break;
                    }
                    ReadAndDumpField(uf, sb, gr, updatetype, data, obj);
                }
            }

            if ((objectTypeId == ObjectTypes.TYPEID_GAMEOBJECT || objectTypeId == ObjectTypes.TYPEID_UNIT) && (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2) && obj.IsNew)
                obj.Save();

            sb.AppendLine("=== values_update_block_end ===");
            return true;
        }

        private static void ReadAndDumpField(UpdateField uf, StringBuilder sb, GenericReader gr, UpdateTypes updatetype, StreamWriter data, WoWObject obj)
        {
            MemoryStream ms = new MemoryStream(gr.ReadBytes(4));
            GenericReader gr2 = new GenericReader(ms);

            if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
            {
                obj.SetUInt32Value(uf.Identifier, gr2.ReadUInt32());
                gr2.BaseStream.Position -= 4;
            }

            switch (uf.Type)
            {
                // TODO: add data writing
                /*case 3:
                    string val1 = gr.ReadSingle().ToString().Replace(",", ".");
                    if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
                        data.WriteLine(uf.Name + " (" + uf.Identifier + "): " + val1);
                    sb.AppendLine(uf.Name + " (" + index + "): " + val1);
                    break;
                default:
                    uint val2 = gr.ReadUInt32();
                    if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
                        data.WriteLine(uf.Name + " (" + uf.Identifier + "): " + val2);
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + val2);
                    break;*/
                case 1: // uint32
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + gr2.ReadUInt32().ToString("X8"));
                    break;
                case 2: // uint16+uint16
                    ushort value1 = gr2.ReadUInt16();
                    ushort value2 = gr2.ReadUInt16();

                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + "first " + value1.ToString("X4") + ", second " + value2.ToString("X4"));
                    if (uf.Name.StartsWith("PLAYER_SKILL_INFO_1_"))
                    {
                        int num = uf.Identifier - 858;
                        if ((num % 3) == 0)
                        {
                            ushort skill = value1;
                            ushort flag = value2;

                            string str = String.Format("skill {0}, flag {1}", skill, (ProfessionFlags)flag);
                            sb.AppendLine(str);
                        }
                        else if (((num - 1) % 3) == 0)
                        {
                            ushort minskill = value1;
                            ushort maxskill = value2;

                            string str = String.Format("minskill {0}, maxskill {1}", minskill, maxskill);
                            sb.AppendLine(str);
                        }
                        else
                        {
                            ushort minbonus = value1;
                            ushort maxbonus = value2;

                            string str = String.Format("minbonus {0}, maxbonus {1}", minbonus, maxbonus);
                            sb.AppendLine(str);
                        }
                    }
                    break;
                case 3: // float
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + gr2.ReadSingle());
                    //sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + gr.ReadSingle().ToString().Replace(",", "."));
                    break;
                case 4: // uint64 (can be only low part)
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + gr2.ReadUInt32().ToString("X8"));
                    break;
                case 5: // bytes
                    uint value = gr2.ReadUInt32();
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + value.ToString("X8"));
                    if (uf.Identifier == 36) // UNIT_FIELD_BYTES_0
                    {
                        byte[] bytes = BitConverter.GetBytes(value);
                        Races race = (Races)bytes[0];
                        Class class_ = (Class)bytes[1];
                        Gender gender = (Gender)bytes[2];
                        Powers powertype = (Powers)bytes[3];

                        string str = String.Format("Race: {0}, class: {1}, gender: {2}, powertype: {3}", race, class_, gender, powertype);
                        sb.AppendLine(str);
                    }
                    break;
                default:
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + "unknown type " + gr2.ReadUInt32().ToString("X8"));
                    break;
            }

            gr2.Close();
        }

        private static bool ParseMovementUpdateBlock(GenericReader gr, StringBuilder sb, StreamWriter swe, StreamWriter data, ObjectTypes objectTypeId, WoWObject obj)
        {
            Coords4 coords;
            coords.X = 0;
            coords.Y = 0;
            coords.Z = 0;
            coords.O = 0;

            sb.AppendLine("=== movement_update_block_start ===");
            MovementFlags mf = MovementFlags.MOVEMENTFLAG_NONE;    // movement flags

            UpdateFlags flags = (UpdateFlags)gr.ReadByte();
            sb.AppendLine("Update Flags: " + flags.ToString("X") + " : " + flags);

            if ((UpdateFlags.UPDATEFLAG_LIVING & flags) != 0) // 0x20
            {
                mf = (MovementFlags)gr.ReadUInt32();
                sb.AppendLine("Movement Flags: " + mf.ToString("X") + " : " + mf);

                byte unk = gr.ReadByte();
                sb.AppendLine("Unknown Byte: " + unk.ToString("X2"));

                uint time = gr.ReadUInt32();
                sb.AppendLine("Time: " + time.ToString("X8"));
            }

            if ((UpdateFlags.UPDATEFLAG_HASPOSITION & flags) != 0) // 0x40
            {
                coords = gr.ReadCoords4();
                sb.AppendLine("Coords: " + coords.GetCoordsAsString());

                if (objectTypeId == ObjectTypes.TYPEID_UNIT || objectTypeId == ObjectTypes.TYPEID_GAMEOBJECT)
                {
                    if (obj != null)
                        obj.SetPosition(coords.X, coords.Y, coords.Z, coords.O);
                }
            }

            if ((flags & UpdateFlags.UPDATEFLAG_LIVING) != 0)   // 0x20
            {
                /*if (objectTypeId == ObjectTypes.TYPEID_UNIT || objectTypeId == ObjectTypes.TYPEID_GAMEOBJECT)
                {
                    data.WriteLine();
                    data.WriteLine(objectTypeId + ": " + coords.GetCoordsAsString());
                }*/

                if ((mf & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != 0) // transport
                {
                    ulong t_guid = gr.ReadUInt64();
                    sb.Append("Transport GUID: " + t_guid.ToString("X16") + ", ");

                    Coords4 transport = gr.ReadCoords4();
                    sb.AppendLine("Transport Coords: " + transport.GetCoordsAsString());

                    uint unk2 = gr.ReadUInt32(); // unk, probably timestamp
                    sb.AppendLine("Transport Unk: " + unk2.ToString("X8"));
                }

                if ((mf & (MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_UNK5)) != 0)
                {
                    float unkf1 = gr.ReadSingle();
                    sb.AppendLine("MovementFlags & (MOVEMENTFLAG_SWIMMING | MOVEMENTFLAG_UNK5): " + unkf1);
                }

                uint unk1 = gr.ReadUInt32();
                sb.AppendLine("Unk1: " + unk1.ToString("X8"));

                if ((mf & MovementFlags.MOVEMENTFLAG_JUMPING) != 0)
                {
                    // looks like orientation/coords/speed
                    float unk3 = gr.ReadSingle();
                    sb.AppendLine("unk3: " + unk3);
                    float unk4 = gr.ReadSingle();
                    sb.AppendLine("unk4: " + unk4);
                    float unk5 = gr.ReadSingle();
                    sb.AppendLine("unk5: " + unk5);
                    float unk6 = gr.ReadSingle();
                    sb.AppendLine("unk6: " + unk6);
                }

                if ((mf & MovementFlags.MOVEMENTFLAG_SPLINE) != 0)
                {
                    float unkf2 = gr.ReadSingle();
                    sb.AppendLine("MovementFlags & MOVEMENTFLAG_SPLINE: " + unkf2);
                }

                float ws = gr.ReadSingle();
                sb.AppendLine("Walk speed: " + ws);
                float rs = gr.ReadSingle();
                sb.AppendLine("Run speed: " + rs);
                float sbs = gr.ReadSingle();
                sb.AppendLine("Swimback speed: " + sbs);
                float ss = gr.ReadSingle();
                sb.AppendLine("Swim speed: " + ss);
                float wbs = gr.ReadSingle();
                sb.AppendLine("Walkback speed: " + wbs);
                float fs = gr.ReadSingle();
                sb.AppendLine("Fly speed: " + fs);
                float fbs = gr.ReadSingle();
                sb.AppendLine("Flyback speed: " + fbs);
                float ts = gr.ReadSingle();
                sb.AppendLine("Turn speed: " + ts); // pi = 3.14

                if ((mf & MovementFlags.MOVEMENTFLAG_SPLINE2) != 0)
                {
                    uint flags3 = gr.ReadUInt32();
                    sb.AppendLine("SplineFlags " + flags3.ToString("X8"));

                    if ((flags3 & 0x10000) != 0)
                    {
                        Coords3 c = gr.ReadCoords3();
                        sb.AppendLine("SplineFlags & 0x10000: " + c.GetCoords());
                    }
                    if ((flags3 & 0x20000) != 0)
                    {
                        ulong g3 = gr.ReadUInt64();
                        sb.AppendLine("flags3_guid: " + g3.ToString("X16")); // ????
                    }
                    if ((flags3 & 0x40000) != 0)
                    {
                        uint f3_3 = gr.ReadUInt32();
                        sb.AppendLine("flags3_unk_value3: " + f3_3.ToString("X8"));
                    }

                    uint t1 = gr.ReadUInt32();
                    sb.AppendLine("curr tick: " + t1.ToString("X8"));

                    uint t2 = gr.ReadUInt32();
                    sb.AppendLine("last tick: " + t2.ToString("X8"));

                    uint t3 = gr.ReadUInt32();
                    sb.AppendLine("tick count " + t3.ToString("X8"));

                    uint coords_count = gr.ReadUInt32();
                    sb.AppendLine("coords_count: " + coords_count.ToString("X8"));

                    for (uint i = 0; i < coords_count; i++)
                    {
                        Coords3 v = gr.ReadCoords3();
                        sb.AppendLine("coord" + i + ": " + v.GetCoords());
                    }

                    Coords3 end = gr.ReadCoords3();
                    sb.AppendLine("end: " + end.GetCoords());
                }
            }

            if ((flags & UpdateFlags.UPDATEFLAG_LOWGUID) != 0)   // 0x08
            {
                uint temp = gr.ReadUInt32(); // timestamp or something like it
                sb.AppendLine("UpdateFlags & 0x08 (lowguid): " + temp.ToString("X8"));
            }

            if ((UpdateFlags.UPDATEFLAG_HIGHGUID & flags) != 0) // 0x10
            {
                uint guid_high = gr.ReadUInt32(); // timestamp or something like it
                sb.AppendLine("UpdateFlags & 0x10 (highguid): " + guid_high.ToString("X8"));
            }

            if ((UpdateFlags.UPDATEFLAG_FULLGUID & flags) != 0) // 0x04
            {
                ulong guid2 = gr.ReadPackedGuid(); // guid, but what guid?
                sb.AppendLine("UpdateFlags & 0x04 guid: " + guid2.ToString("X16"));
            }

            if ((UpdateFlags.UPDATEFLAG_TRANSPORT & flags) != 0) // 0x02
            {
                uint time = gr.ReadUInt32(); // time
                sb.AppendLine("UpdateFlags & 0x02 t_time: " + time.ToString("X8"));
            }

            if ((UpdateFlags.UPDATEFLAG_SELFTARGET & flags) != 0) // 0x01
            {
                sb.AppendLine("updating self!");
            }

            sb.AppendLine("=== movement_update_block_end ===");
            return true;
        }
    }
}
