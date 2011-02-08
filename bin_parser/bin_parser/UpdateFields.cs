using System;
using System.Collections.Generic;
using System.IO;

namespace UpdateFields
{
    #region UpdateTypes
    /// <summary>
    /// WoW Update Types.
    /// </summary>
    public enum UpdateTypes
    {
        /// <summary>
        /// Update type that update only object field values.
        /// </summary>
        UPDATETYPE_VALUES = 0,
        /// <summary>
        /// Update type that update only object movement.
        /// </summary>
        UPDATETYPE_MOVEMENT = 1,
        /// <summary>
        /// Update type that create an object (full update).
        /// </summary>
        UPDATETYPE_CREATE_OBJECT = 2,
        /// <summary>
        /// Update type that create an object (gull update, self use).
        /// </summary>
        UPDATETYPE_CREATE_OBJECT2 = 3,
        /// <summary>
        /// Update type that update only objects out of range.
        /// </summary>
        UPDATETYPE_OUT_OF_RANGE_OBJECTS = 4,
        /// <summary>
        /// Update type that update only near objects.
        /// </summary>
        UPDATETYPE_NEAR_OBJECTS = 5,
    }
    #endregion

    #region UpdateFlags
    /// <summary>
    /// WoW Update Flags
    /// </summary>
    [Flags]
    public enum UpdateFlags
    {
        /// <summary>
        /// Update flag for self.
        /// </summary>
        UPDATEFLAG_SELFTARGET = 0x01,
        /// <summary>
        /// Update flag for transport object.
        /// </summary>
        UPDATEFLAG_TRANSPORT = 0x02,
        /// <summary>
        /// Update flag with full guid.
        /// </summary>
        UPDATEFLAG_FULLGUID = 0x04,
        /// <summary>
        /// Update flag unknown...
        /// </summary>
        UPDATEFLAG_LOWGUID = 0x08,
        /// <summary>
        /// Common update flag.
        /// </summary>
        UPDATEFLAG_HIGHGUID = 0x10,
        /// <summary>
        /// Update flag for living objects.
        /// </summary>
        UPDATEFLAG_LIVING = 0x20,
        /// <summary>
        /// Update flag for world objects (players, units, go, do, corpses).
        /// </summary>
        UPDATEFLAG_HASPOSITION = 0x40
    }
    #endregion

    #region ObjectTypes
    /// <summary>
    /// WoW object types.
    /// </summary>
    public enum ObjectTypes : uint
    {
        /// <summary>
        /// An object.
        /// </summary>
        TYPEID_OBJECT = 0,
        /// <summary>
        /// Item.
        /// </summary>
        TYPEID_ITEM = 1,
        /// <summary>
        /// Container (item).
        /// </summary>
        TYPEID_CONTAINER = 2,
        /// <summary>
        /// Unit.
        /// </summary>
        TYPEID_UNIT = 3,
        /// <summary>
        /// Player (unit).
        /// </summary>
        TYPEID_PLAYER = 4,
        /// <summary>
        /// Game object.
        /// </summary>
        TYPEID_GAMEOBJECT = 5,
        /// <summary>
        /// Dynamic object.
        /// </summary>
        TYPEID_DYNAMICOBJECT = 6,
        /// <summary>
        /// Player corpse (not used for units?).
        /// </summary>
        TYPEID_CORPSE = 7,
        /// <summary>
        /// It really exist?
        /// </summary>
        TYPEID_AIGROUP = 8,
        /// <summary>
        /// It really exist?
        /// </summary>
        TYPEID_AREATRIGGER = 9,
    }
    #endregion

    #region Updatefield struct
    public struct UpdateField
    {
        public int Identifier;
        public string Name;
        public uint Type;
        public uint Format;
        public uint Value;

        public UpdateField(int id, string name, uint type, uint format, uint value)
        {
            Identifier = id;
            Name = name;
            Type = type;
            Format = format;
            Value = value;
        }
    }
    #endregion

    public class UpdateFieldsLoader
    {
        /// <summary>
        /// Object update fields end.
        /// </summary>
        public static uint OBJECT_END = 6;
        /// <summary>
        /// Item update fields end.
        /// </summary>
        public static uint ITEM_END = 60;
        /// <summary>
        /// Container update fields end.
        /// </summary>
        public static uint CONTAINER_END = 134;
        /// <summary>
        /// Unit update fields end.
        /// </summary>
        public static uint UNIT_END = 234; // 2.3.0 - 232
        /// <summary>
        /// Player updatefields end.
        /// </summary>
        public static uint PLAYER_END = 1528; // 2.3.0 - 1450
        /// <summary>
        /// Game object update fields end.
        /// </summary>
        public static uint GO_END = 26;
        /// <summary>
        /// Dynamic object fields end.
        /// </summary>
        public static uint DO_END = 16;
        /// <summary>
        /// Corpse fields end.
        /// </summary>
        public static uint CORPSE_END = 38;

        static byte type = 0;

        public static Dictionary<int, UpdateField> item_uf = new Dictionary<int, UpdateField>(); // item + container
        public static Dictionary<int, UpdateField> unit_uf = new Dictionary<int, UpdateField>(); // unit + player
        public static Dictionary<int, UpdateField> go_uf = new Dictionary<int, UpdateField>();
        public static Dictionary<int, UpdateField> do_uf = new Dictionary<int, UpdateField>();
        public static Dictionary<int, UpdateField> corpse_uf = new Dictionary<int, UpdateField>();

        public static void LoadUpdateFields()
        {
            StreamReader sr = new StreamReader("updatefields_240.dat");
            while (sr.Peek() >= 0)
            {
                string curline = sr.ReadLine();

                if (curline.StartsWith("#") || curline.StartsWith("/")) // skip commentary lines
                    continue;

                if (curline.Length == 0)    // empty line
                    continue;

                if (curline.StartsWith(":"))    // label lines
                {
                    if (curline.Contains("unit+player"))
                        type = 1;
                    else if (curline.Contains("gameobject"))
                        type = 2;
                    else if (curline.Contains("dynamicobject"))
                        type = 3;
                    else if (curline.Contains("corpse"))
                        type = 4;

                    continue;
                }

                string[] arr = curline.Split('	');

                if (arr.Length < 3)
                    continue;

                int id = Convert.ToInt32(arr[0]);
                string name = arr[1];
                uint type1 = Convert.ToUInt32(arr[2]);
                //uint format = Convert.ToUInt32(arr[3]);
                uint format = 0;

                UpdateField uf = new UpdateField(id, name, type1, format, 0);
                switch (type)
                {
                    case 0:
                        item_uf.Add(id, uf);
                        break;
                    case 1:
                        unit_uf.Add(id, uf);
                        break;
                    case 2:
                        go_uf.Add(id, uf);
                        break;
                    case 3:
                        do_uf.Add(id, uf);
                        break;
                    case 4:
                        corpse_uf.Add(id, uf);
                        break;
                }
            }

            CheckIntegrity();
        }

        /// <summary>
        /// Check updatefields.dat integrity
        /// </summary>
        public static void CheckIntegrity()
        {
            // program will crash there if updatefields.dat contains errors
            for (int i = 0; i < item_uf.Count; i++)
            {
                UpdateField uf = item_uf[i];
            }

            for (int i = 0; i < unit_uf.Count; i++)
            {
                UpdateField uf = unit_uf[i];
            }

            for (int i = 0; i < go_uf.Count; i++)
            {
                UpdateField uf = go_uf[i];
            }

            for (int i = 0; i < do_uf.Count; i++)
            {
                UpdateField uf = do_uf[i];
            }

            for (int i = 0; i < corpse_uf.Count; i++)
            {
                UpdateField uf = corpse_uf[i];
            }
        }
    }
}
