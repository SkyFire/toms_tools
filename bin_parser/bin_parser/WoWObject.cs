using System;
using System.IO;
using System.Text;
using UpdateFields;

namespace WoWObjects
{
    /// <summary>
    /// WoW Object Class
    /// </summary>
    public class WoWObject
    {
        uint m_valuesCount;
        uint[] m_uint32Values;
        ObjectTypes m_typeId;
        bool m_new;

        // position
        float x, y, z, o;

        /// <summary>
        /// WoWObject constructor
        /// </summary>
        /// <param name="valuesCount">Number of fields</param>
        /// <param name="typeId">Object typeid</param>
        public WoWObject(uint valuesCount, ObjectTypes typeId)
        {
            if (m_valuesCount != 0)
                m_valuesCount = valuesCount;
            else
                m_valuesCount = GetValuesCountByObjectType(typeId);

            m_uint32Values = new uint[m_valuesCount];
            m_typeId = typeId;
        }

        uint GetValuesCountByObjectType(ObjectTypes typeId)
        {
            switch (typeId)
            {
                case ObjectTypes.TYPEID_ITEM:
                    return UpdateFieldsLoader.ITEM_END;
                case ObjectTypes.TYPEID_CONTAINER:
                    return UpdateFieldsLoader.CONTAINER_END;
                case ObjectTypes.TYPEID_UNIT:
                    return UpdateFieldsLoader.UNIT_END;
                case ObjectTypes.TYPEID_PLAYER:
                    return UpdateFieldsLoader.PLAYER_END;
                case ObjectTypes.TYPEID_GAMEOBJECT:
                    return UpdateFieldsLoader.GO_END;
                case ObjectTypes.TYPEID_DYNAMICOBJECT:
                    return UpdateFieldsLoader.DO_END;
                case ObjectTypes.TYPEID_CORPSE:
                    return UpdateFieldsLoader.CORPSE_END;
                default:
                    return 0;
            }
        }

        public WoWObject()
        {
            m_valuesCount = 0;
            m_uint32Values = null;
            m_typeId = ObjectTypes.TYPEID_OBJECT;
        }

        public void SetPosition(float X, float Y, float Z, float O)
        {
            x = X;
            y = Y;
            z = Z;
            o = O;
        }

        public bool IsNew
        {
            get { return m_new; }
            set { m_new = value; }
        }

        public ObjectTypes TypeId
        {
            get { return m_typeId; }
            set { m_typeId = value; }
        }

        public uint ValuesCount
        {
            get { return m_valuesCount; }
            set { m_valuesCount = value; }
        }

        public uint[] UInt32Values
        {
            get { return m_uint32Values; }
            set { m_uint32Values = value; }
        }

        public ulong GetGUID()
        {
            return GetUInt64Value(0);
        }

        public uint GetGUIDLow()
        {
            return m_uint32Values[0];
        }

        public uint GetGUIDHigh()
        {
            return m_uint32Values[1];
        }

        public uint GetUInt32Value(int index)
        {
            return m_uint32Values[index];
        }

        public ulong GetUInt64Value(int index)
        {
            ulong result = 0;

            result += m_uint32Values[index];
            result += ((ulong)m_uint32Values[index + 1] << 32);

            return result;
        }

        public float GetFloatValue(int index)
        {
            byte[] temp = BitConverter.GetBytes(m_uint32Values[index]);
            return BitConverter.ToSingle(temp, 0);
        }

        public void SetUInt32Value(int index, uint value)
        {
            m_uint32Values[index] = value;
        }

        public void SetUInt64Value(int index, ulong value)
        {
            uint low = (uint)value;
            uint high = (uint)(value >> 32);

            m_uint32Values[index] = low;
            m_uint32Values[index + 1] = high;
        }

        public void SetFloatValue(int index, float value)
        {
            byte[] temp = BitConverter.GetBytes(value);
            m_uint32Values[index] = BitConverter.ToUInt32(temp, 0);
        }

        public void LoadValues(string data)
        {
            string[] values = data.Split(' ');

            for (ushort i = 0; i < m_valuesCount; i++)
                m_uint32Values[i] = Convert.ToUInt32(values[i]);
        }

        public void Save()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("INSERT INTO `objects` VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '", GetGUIDHigh(), GetGUIDLow(), x, y, z, o, (int)m_typeId);

            for (ushort i = 0; i < m_valuesCount; i++)
            {
                sb.AppendFormat("{0} ", GetUInt32Value(i));
            }

            sb.Append("');");

            StreamWriter sw = new StreamWriter("data.sql", true);
            sw.WriteLine(sb.ToString());
            sw.Flush();
            sw.Close();
        }
    }

    // item
    public class Item : WoWObject
    {
        public Item()
        {
            ValuesCount = UpdateFieldsLoader.ITEM_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_ITEM;
        }
    }

    // container
    public class Container : WoWObject
    {
        public Container()
        {
            ValuesCount = UpdateFieldsLoader.CONTAINER_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_CONTAINER;
        }
    }

    // unit
    public class Unit : WoWObject
    {
        public Unit()
        {
            ValuesCount = UpdateFieldsLoader.UNIT_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_UNIT;
        }
    }

    // player
    public class Player : Unit
    {
        public Player()
        {
            ValuesCount = UpdateFieldsLoader.PLAYER_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_PLAYER;
        }
    }

    // gameobject
    public class GameObject : WoWObject
    {
        public GameObject()
        {
            ValuesCount = UpdateFieldsLoader.GO_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_GAMEOBJECT;
        }
    }

    // dynamicobject
    public class DynamicObject : WoWObject
    {
        public DynamicObject()
        {
            ValuesCount = UpdateFieldsLoader.DO_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_DYNAMICOBJECT;
        }
    }

    // corpse
    public class Corpse : WoWObject
    {
        public Corpse()
        {
            ValuesCount = UpdateFieldsLoader.CORPSE_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_CORPSE;
        }
    }
}
