using System;
using System.Text;

namespace a9_parser
{
	#region BitArr
	/// <summary>
	/// This class implements bit array logic.
	/// </summary>
	public class BitArr : ICloneable, IDisposable
	{
		#region Std Interfaces
		/// <summary>
		/// Clear all references to other objects.
		/// </summary>
		public void Dispose()
		{
			size = 0;
			words = null;
		}

		/// <summary>
		/// Creates a new object that is a deep copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a deep copy of this instance.</returns>
		public object Clone()
		{
			BitArr ret = new BitArr();
			if (words != null)
				ret.words = (uint[])words.Clone();
			else
				ret.words = null;
			ret.size = size;
			return ret;
		}

		#endregion

		#region Object's variables
		/// <summary>
		/// Array to store data.
		/// </summary>
		protected uint[] words;

		/// <summary>
		/// Variable for size storing (in bits!).
		/// </summary>
		protected int size;

		/// <summary>
		/// The size of single word in data array.
		/// </summary>
		protected const int word_size = 32;

		/// <summary>
		/// Masks to cut rest of byte's data.
		/// </summary>
		protected static readonly uint[] cutmasks = {
														0x1, 0x3, 0x7, 0xF, 0x1F, 0x3F, 0x7F, 0xFF,
														0x1FF, 0x3FF, 0x7FF, 0xFFF, 0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF,
														0x1FFFF, 0x3FFFF, 0x7FFFF, 0xFFFFF, 0x1FFFFF, 0x3FFFFF, 0x7FFFFF, 0xFFFFFF,
														0x1FFFFFF, 0x3FFFFFF, 0x7FFFFFF, 0xFFFFFFF, 0x1FFFFFFF, 0x3FFFFFFF, 0x7FFFFFFF, 0xFFFFFFFF};

		/// <summary>
		/// Hight bit of the word.
		/// </summary>
		protected const uint word_hbit = 0x80000000;

		/// <summary>
		/// Next bit to hight bit of the word.
		/// </summary>
		protected const long word_hbitplus = 0x100000000;

		/// <summary>
		/// Randomizator for rand() method.
		/// </summary>
		static Random randomizator = new Random();
		#endregion

		#region Constructors
		/// <summary>
		/// It is default constructor.
		/// </summary>
		public BitArr()
		{
			words = null;
			size = 0;
		}

		/// <summary>
		/// It is constructor.
		/// </summary>
		/// <param name="size">Inital array size.</param>
		public BitArr(int size)
		{
			if (size == 0) return;
			this.size = size;
			words = new uint[BS2WS(size)];
		}

		/// <summary>
		/// Constructor from bits array.
		/// </summary>
		/// <param name="bits">Bits or bits array.</param>
		public BitArr(params bool[] bits)
			: this(bits.Length)
		{
			for (int i=0; i<size; i++)
				if (bits[i]) this[i] = bits[i];
		}

		/// <summary>
		/// Constructor from a string.
		/// </summary>
		/// <param name="bits">String with '1's and '0's.</param>
		public BitArr(string bits)
			: this(bits.Length)
		{
			for (int i=0; i<size; i++)
				switch (bits[i])
				{
					case '0':
						break;
					case '1':
						this[i] = true;
						break;
					default:
						throw new ArgumentException("Must be '0's and '1's string.");
				}
		}
		#endregion

		#region Methods and properties
		/// <summary>
		/// Transforms size in bits into size in words.
		/// </summary>
		/// <param name="bits">Size in bits.</param>
		/// <returns>Size in words.</returns>
		protected static int BS2WS(int bits)
		{
			return (bits + word_size-1) / word_size;
		}
		
		/// <summary>
		/// Resize an array.
		/// </summary>
		/// <param name="newsize">New size (in bits!).</param>
		public void ReSize(int newsize)
		{
			// Checking newsize:
			if (newsize == 0)
			{
				words = null;
				size = 0;
				return;
			}
			else if (newsize < 0)
				throw new ArgumentOutOfRangeException();

			// Resizing bytes array:
			int newwsize = BS2WS(newsize);
			if (words==null || newwsize>words.Length)
			{
				uint[] newarray = new uint[newwsize];
				if (words!=null) words.CopyTo(newarray, 0);
				words = newarray;
			}
			else if(newwsize < words.Length)
			{
				uint[] newarray = new uint[newwsize];
				for (int i=0; i<newwsize; i++)
					newarray[i] = words[i];
				words = newarray;
			}
			words[newwsize-1] &= cutmasks[(newsize-1)%word_size];

			// Changing size:
			size = newsize;
		}

		/// <summary>
		/// Indexer for the array. (recalls this[int index])
		/// </summary>
		public bool this[uint index]
		{
			set
			{
				this[(int)index] = value;
			}
			get
			{
				return this[(int)index];
			}
		}

		/// <summary>
		/// Indexer for the array.
		/// </summary>
		public bool this[int index]
		{
			set
			{
				if (index >= size || index < 0) throw new ArgumentOutOfRangeException();
				uint mask = (uint)(1 << (index%word_size));
				int windex = index/word_size;
				if (value) words[windex] |= mask;
				else words[windex] &= (byte)(~mask);
			}
			get
			{
				if (index >= size || index < 0) throw new ArgumentOutOfRangeException();
				if (index > size || index < 0)
					throw new System.IndexOutOfRangeException();
				uint mask = (uint)(1 << (index%word_size));
				int windex = index/word_size;
				return (words[windex]&mask)!=0;
			}
		}

		/// <summary>
		/// Returns or sets length of an array (in bits).
		/// </summary>
		public int Length
		{
			get
			{
				return size;
			}
			set
			{
				ReSize(value);
			}
		}

		/// <summary>
		/// Returns number of highest bit in word.
		/// </summary>
		/// <param name="w">Input word to analyze.</param>
		/// <returns>Count of signified bits.</returns>
		protected int HighBit(uint w)
		{
			if (w==0) return 0;
			int ret = 1;
			while (w != (w&cutmasks[ret-1]))
				ret++;
			return ret;
		}
		
		/// <summary>
		/// Count of signified bits. Returns number of highest bit. Readonly.
		/// </summary>
		public int RealLength
		{
			get
			{
				if (Length == 0)
					return 0;
				int ret = 0;
				int i;
				for (i=this.words.Length-1; i>=0; i--)
					if (this.words[i] != 0)
						break;
				if (i<0) return 0;
				if (i>0) ret += i*word_size;
				ret += HighBit(this.words[i]);
				return ret;
			}
		}

		/// <summary>
		/// Is this empty or zeros array?
		/// </summary>
		/// <remarks>Readonly.</remarks>
		public bool IsZero
		{
			get
			{
				return RealLength == 0;
			}
		}

		/// <summary>
		/// Performs memberwise copy from BitArr.
		/// </summary>
		/// <param name="ba">Source BitArr.</param>
		/// <remarks>I use it to avoid ****ing "Compiler Error CS1540" in derived
		/// class's cast methods.</remarks>
		protected void Assign(BitArr ba)
		{
			this.words = ba.words;
			this.size = ba.size;
		}

		/// <summary>
		/// Fills array with random bits.
		/// </summary>
		public void rand()
		{
			if (words == null)
				throw new ArgumentNullException("Can't fill null array!");
			int binw = word_size/8;
			byte[] bytes = new byte[(size+7)/8];
			randomizator.NextBytes(bytes);
			for (int i=0; i<words.Length; i++)
				for (int j=0; (j<binw) && ( (i*binw+j)*8 <= size ); j++)
					words[i] = (words[i]<<8) + bytes[i*binw+j];
			words[(size-1)/word_size] &= cutmasks[(size-1)%word_size];
		}
		#endregion

		#region Overrided methods
		/// <summary>
		/// Converts bits array into string.
		/// </summary>
		/// <returns>String with '1's and '0's.</returns>
		/// <remarks>BigEndian</remarks>
		public string ToString01()
		{
			if (size == 0)
				return "empty";
			StringBuilder sb = new StringBuilder(size);
			for (int i=0; i<size; i++)
				sb.Append(this[i] ? '1' : '0');
			return sb.ToString();
		}

		/// <summary>
		/// Converts bits array into string.
		/// </summary>
		/// <returns>String with '1's and '0's.</returns>
		/// <remarks>LowEndian</remarks>
		public string ToString10()
		{
			if (size == 0)
				return "empty";
			StringBuilder sb = new StringBuilder(size);
			for (int i=size-1; i>=0; i--)
				sb.Append(this[i] ? '1' : '0');
			return sb.ToString();
		}

		/// <summary>
		/// Converts bits array into string.
		/// </summary>
		/// <returns>String in hex.</returns>
		/// <remarks>LowEndian</remarks>
		public string ToStringHex()
		{
			if (size == 0)
				return "empty";
			StringBuilder sb = new StringBuilder(words.Length * (word_size/8) * 2);
			for (int i=words.Length-1; i>=0; i--)
				sb.Append(words[i].ToString("X2"));
			return sb.ToString();
		}

		/// <summary>
		/// Converts bits array into string.
		/// </summary>
		/// <returns>String.</returns>
		/// <remarks>Calls ToStringHex().</remarks>
		public override string ToString()
		{
			return ToStringHex();
		}

		/// <summary>
		/// Serves as a hash function.
		/// </summary>
		/// <returns>A hash code for the current bits.</returns>
		/// <remarks>Hash code depends on array length too.</remarks>
		public override Int32 GetHashCode()
		{
			int ret = base.GetHashCode();
			ret ^= size ^ ((~size)<<16);
			if (words != null)
				for (int i=0; i<words.Length; i++)
					ret ^= words[i].GetHashCode();
			return ret;
		}

		/// <summary>
		/// Comparison of two bits arrays.
		/// </summary>
		/// <param name="a">An other bits array.</param>
		/// <returns>True, is arrays equals.</returns>
		public override bool Equals(object a)
		{
			if (a == null) return false;
			if (a.GetType() != this.GetType()) return false;

			BitArr b = (BitArr)a;
			if (b.size != size) return false;
			if (size == 0) return true;
			for (int i=0; i<BS2WS(size); i++)
				if ( b.words[i] != words[i] )
					return false;

			return true;
		}
		#endregion

		#region Convertion operators
		/// <summary>
		/// Converts "1011"-string into BitArr.
		/// </summary>
		/// <param name="bits">String with '1's and '0's.</param>
		/// <returns>Returns BitArr object.</returns>
		public static implicit operator BitArr(string bits)
		{
			return new BitArr(bits);
		}

		/// <summary>
		/// Converts bytes array into BitArr.
		/// </summary>
		/// <param name="bytes">Bytes array.</param>
		/// <returns>Returns BitArr object.</returns>
		/// <remarks>Perfoms shalow copy from array.</remarks>
		public static implicit operator BitArr(byte[] bytes)
		{
			BitArr ret = new BitArr();
			if (bytes == null || bytes.Length==0)
				return ret;

			int binw = word_size/8;
			ret.words = new uint[bytes.Length / binw];

			for (int i=0; i<ret.words.Length; i++)
				for (int j=0; j<binw; j++)
					ret.words[i] = (ret.words[i]<<8) + bytes[i*binw+j];

			ret.size = bytes.Length * 8;
			return ret;
		}

		/// <summary>
		/// Converts uints array into BitArr.
		/// </summary>
		/// <param name="uints">Uints array.</param>
		/// <returns>Returns BitArr object.</returns>
		/// <remarks>Perfoms shalow copy from array.</remarks>
		public static implicit operator BitArr(uint[] uints)
		{
			BitArr ret = new BitArr();
			ret.words = new uint[uints.Length];
			Array.Copy(uints, 0, ret.words, 0, uints.Length);
			ret.size = uints.Length * 32;
			return ret;
		}

		/// <summary>
		/// Converts UInt16 into BitArr.
		/// </summary>
		/// <param name="bits">UInt16 with '1's and '0's.</param>
		/// <returns>Returns BitArr object.</returns>
		public static explicit operator BitArr(System.UInt16 bits)
		{
			BitArr ba = new BitArr(16);
			int i=0;
			for (UInt16 mask=1; i<16; i++, mask<<=1)
				if ((mask&bits)!=0)
					ba[i] = true;
			return ba;
		}

		/// <summary>
		/// Converts UInt32 into BitArr.
		/// </summary>
		/// <param name="bits">UInt32 with '1's and '0's.</param>
		/// <returns>Returns BitArr object.</returns>
		public static explicit operator BitArr(System.UInt32 bits)
		{
			BitArr ba = new BitArr(32);
			int i=0;
			for (UInt32 mask=1; i<32; i++, mask<<=1)
				if ((mask&bits)!=0)
					ba[i] = true;
			return ba;
		}

		/// <summary>
		/// Converts UInt64 into BitArr.
		/// </summary>
		/// <param name="bits">UInt64 with '1's and '0's.</param>
		/// <returns>Returns BitArr object.</returns>
		public static explicit operator BitArr(System.UInt64 bits)
		{
			BitArr ba = new BitArr(64);
			int i=0;
			for (UInt64 mask=1; i<64; i++, mask<<=1)
				if ((mask&bits)!=0)
					ba[i] = true;
			return ba;
		}

		/// <summary>
		/// Converts BitArr into string.
		/// </summary>
		/// <param name="ba">Bits array.</param>
		/// <returns>String.</returns>
		/// <remarks>Calls ToString method.</remarks>
		public static explicit operator string(BitArr ba)
		{
			return ba.ToString();
		}

		/// <summary>
		/// Converts BitArr into bytes array.
		/// </summary>
		/// <param name="ba">Bits array.</param>
		/// <returns>Bytes array.</returns>
		/// <remarks>Creates shallow copy of bytes array.</remarks>
		public static explicit operator byte[](BitArr ba)
		{
			if (ba.size == 0)
				return null;
			
			byte[] bytes = new byte[(ba.size+7)/8];
			for (int i=0; i<bytes.Length; i++)
			{
				int binw = word_size/8;
				int shift = 8 * (i%binw);
				uint b = ba.words[i/word_size] >> shift;
				bytes[i] = (byte)(b&255);
			}

			return bytes;
		}

		/// <summary>
		/// Converts BitArr into uints array.
		/// </summary>
		/// <param name="ba">Bits array.</param>
		/// <returns>Uints array.</returns>
		/// <remarks>Creates shallow copy of uints array.</remarks>
		public static explicit operator uint[](BitArr ba)
		{
			if (ba.size == 0)
				return null;

			uint[] uints = new uint[(ba.size+31)/32];
			Array.Copy(ba.words, 0, uints, 0, uints.Length); 
			return uints;
		}

		/// <summary>
		/// Converts bits array into Int16.
		/// </summary>
		/// <param name="ba">Bits array.</param>
		/// <returns>Int16 or OverflowException.</returns>
		/// <remarks>Int16 can hold 15 bits (use UInt16).</remarks>
		public static explicit operator System.Int16(BitArr ba)
		{
			if (ba.RealLength > 15) throw new OverflowException("Bits array too long for convertion into Int16");
			Int16 ret = (Int16)ba.words[0];
			return ret;
		}

		/// <summary>
		/// Converts bits array into UInt16.
		/// </summary>
		/// <param name="ba">Bits array.</param>
		/// <returns>UInt16 or OverflowException.</returns>
		public static explicit operator System.UInt16(BitArr ba)
		{
			if (ba.RealLength > 16) throw new OverflowException("Bits array too long for convertion into UInt16");
			UInt16 ret = (UInt16)ba.words[0];
			return ret;
		}

		/// <summary>
		/// Converts bits array into Int32.
		/// </summary>
		/// <param name="ba">Bits array.</param>
		/// <returns>Int32 or OverflowException.</returns>
		/// <remarks>Int32 can hold 31 bits (use UInt32).</remarks>
		public static explicit operator System.Int32(BitArr ba)
		{
			if (ba.RealLength > 31) throw new OverflowException("Bits array too long for convertion into Int32");
			Int32 ret = (Int32)ba.words[0];
			return ret;
		}

		/// <summary>
		/// Converts bits array into UInt32.
		/// </summary>
		/// <param name="ba">Bits array.</param>
		/// <returns>UInt32 or OverflowException.</returns>
		public static explicit operator System.UInt32(BitArr ba)
		{
			if (ba.RealLength > 32) throw new OverflowException("Bits array too long for convertion into UInt32");
			UInt32 ret = ba.words[0];
			return ret;
		}

		/// <summary>
		/// Converts bits array into Int64.
		/// </summary>
		/// <param name="ba">Bits array.</param>
		/// <returns>Int64 or OverflowException.</returns>
		/// <remarks>Int64 can hold 63 bits (use UInt64).</remarks>
		public static explicit operator System.Int64(BitArr ba)
		{
			if (ba.RealLength > 63) throw new OverflowException("Bits array too long for convertion into Int64");
			Int64 ret = (Int64)((ba.words[1]<<32) + ba.words[0]);
			return ret;
		}

		/// <summary>
		/// Converts bits array into UInt64.
		/// </summary>
		/// <param name="ba">Bits array.</param>
		/// <returns>UInt32 or OverflowException.</returns>
		public static explicit operator System.UInt64(BitArr ba)
		{
			if (ba.RealLength > 64) throw new OverflowException("Bits array too long for convertion into UInt64");
			UInt64 ret = (UInt64)((ba.words[1]<<32) + ba.words[0]);
			return ret;
		}

		#endregion

		#region Operator overloading
		/// <summary>
		/// Comparison.
		/// </summary>
		/// <param name="a">First bits array.</param>
		/// <param name="b">Second bits array.</param>
		/// <returns>True if equals.</returns>
		public static bool operator==(BitArr a, BitArr b)
		{
			if ((object)a == null && (object)b == null)
				return true;
			else if ((object)a == null)
				return false;
			return a.Equals(b);
		}

		/// <summary>
		/// Negative comparison.
		/// </summary>
		/// <param name="a">First bits array.</param>
		/// <param name="b">Second bits array.</param>
		/// <returns>True if not equals.</returns>
		public static bool operator!=(BitArr a, BitArr b)
		{
			return !a.Equals(b);
		}

		/// <summary>
		/// Bitwise Or.
		/// </summary>
		/// <param name="a">First bits array.</param>
		/// <param name="b">Second bits array.</param>
		/// <returns>a bitwise or b</returns>
		public static BitArr operator|(BitArr a, BitArr b)
		{
			if (a == null && b==null)
				return null;
			else if (a == null)
				return (BitArr)b.Clone();
			else if (b == null)
				return (BitArr)a.Clone();

			int rla = a.RealLength;
			int rlb = b.RealLength;
			int rlc = Math.Max(rla, rlb);

			int wla = BS2WS(rla);
			int wlb = BS2WS(rlb);
			int wlc = BS2WS(rlc);

			BitArr c = new BitArr(rlc);

			for (int i=0; i<wlc; i++)
				if (i > wla)
                    c.words[i] = b.words[i];
				else if (i > wlb)
					c.words[i] = a.words[i];
				else
					c.words[i] = a.words[i] | b.words[i];
			return c;
		}

		#endregion
	}
	#endregion
}
