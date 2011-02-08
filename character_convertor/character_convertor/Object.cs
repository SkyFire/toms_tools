using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace CharacterConverter {
	class Object {
		ushort m_valuesCount;
		uint[] m_uint32Values;

		public Object(ushort valuescount) {
			m_valuesCount = valuescount;

			m_uint32Values = new uint[m_valuesCount];
		}

		~Object() {

		}

		public ushort ValuesCount {
			get { return m_valuesCount; }
		}

		public bool LoadValues(string data) {
			string[] values = data.Trim().Split(' ');

			if(values.Length == (int)UpdateFieldsNew.PLAYER_END) {
				Console.WriteLine("Character already converted, skipping...");
				return false;
			}

			if(values.Length != (int)UpdateFieldsOld.PLAYER_END) {
				Console.WriteLine("Broken character, it has {0} fields instead of {1}, skipped...", values.Length, (int)UpdateFieldsOld.PLAYER_END);
				return false;
			}

			for(ushort i = 0; i < m_valuesCount; ++i)
				m_uint32Values[i] = Convert.ToUInt32(values[i]);

			return true;
		}

		public uint GetGUIDLow() {
			return m_uint32Values[(int)UpdateFieldsNew.OBJECT_FIELD_GUID];
		}

		public uint GetGUIDHigh() {
			return m_uint32Values[(int)UpdateFieldsNew.OBJECT_FIELD_GUID + 1];
		}

		public uint GetUInt32Value(ushort index) {
			return m_uint32Values[index];
		}

		public ulong GetUInt64Value(ushort index) {
			uint low = m_uint32Values[index];
			uint high = m_uint32Values[index + 1];
			return (ulong)(low | (high << 32));
		}

		public float GetFloatValue(ushort index) {
			byte[] temp = BitConverter.GetBytes(m_uint32Values[index]);
			return BitConverter.ToSingle(temp, 0);
		}

		public uint GetUInt32Value(UpdateFieldsOld index) {
			return m_uint32Values[(int)index];
		}

		public ulong GetUInt64Value(UpdateFieldsOld index) {
			uint low = m_uint32Values[(int)index];
			uint high = m_uint32Values[(int)index + 1];
			return (ulong)(low | (high << 32));
		}

		public float GetFloatValue(UpdateFieldsOld index) {
			byte[] temp = BitConverter.GetBytes(m_uint32Values[(int)index]);
			return BitConverter.ToSingle(temp, 0);
		}

		public void SetUInt32Value(ushort index, uint value) {
			m_uint32Values[index] = value;
		}

		public void SetUInt64Value(ushort index, ulong value) {
			m_uint32Values[index] = (uint)(value & 0xFFFFFFFF);
			m_uint32Values[index + 1] = (uint)(value >> 32);
		}

		public void SetFloatValue(ushort index, float value) {
			byte[] temp = BitConverter.GetBytes(value);
			m_uint32Values[index] = BitConverter.ToUInt32(temp, 0);
		}

		public void SetUInt32Value(UpdateFieldsNew index, uint value) {
			m_uint32Values[(int)index] = value;
		}

		public void SetUInt64Value(UpdateFieldsNew index, ulong value) {
			m_uint32Values[(int)index] = (uint)(value & 0xFFFFFFFF);
			m_uint32Values[(int)index + 1] = (uint)(value >> 32);
		}

		public void SetFloatValue(UpdateFieldsNew index, float value) {
			byte[] temp = BitConverter.GetBytes(value);
			m_uint32Values[(int)index] = BitConverter.ToUInt32(temp, 0);
		}
	}
}
