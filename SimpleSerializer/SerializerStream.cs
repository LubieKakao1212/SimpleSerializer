using System;
using System.IO;
using System.Text;

namespace SimpleSerializer
{
    public class SerializerStream : IDisposable
    {
        public readonly Stream outer;
        public SerializationMode currentMode { get; private set; }
        public int SignatureHash { get; private set; }

        public SerializerStream(Stream outer, SerializationMode mode)
        {
            this.outer = outer;
            Reset(mode);
        }

        public void Dispose()
        {
            outer.Dispose();
        }

        public void Reset(SerializationMode mode)
        {
            currentMode = mode;
            SignatureHash = 1009;
        }

        public void ResetHard(SerializationMode mode)
        {
            Reset(mode);
            outer.Position = 0;
        }

        #region primitives
        #region Integers
        #region 1
        public SerializerStream Byte(ref byte value)
        {
            AppendType(PrimitiveType.Byte);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    outer.WriteByte(value);
                    break;
                case SerializationMode.Deserialize:
                    value = (byte)outer.ReadByte();
                    break;
            }
            return this;
        }

        public SerializerStream SByte(ref sbyte value)
        {
            AppendType(PrimitiveType.SByte);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    outer.WriteByte(unchecked((byte)value));
                    break;
                case SerializationMode.Deserialize:
                    value = unchecked((sbyte)outer.ReadByte());
                    break;
            }
            return this;
        }
        #endregion

        #region 2
        public SerializerStream UShort(ref ushort value)
        {
            AppendType(PrimitiveType.UShort);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    Put(BitConverter.GetBytes(value));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToUInt16(Get(new byte[2]), 0);
                    break;
            }
            return this;
        }

        public SerializerStream Short(ref short value)
        {
            AppendType(PrimitiveType.Short);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    Put(BitConverter.GetBytes(value));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToInt16(Get(new byte[2]), 0);
                    break;
            }
            return this;
        }
        #endregion

        #region 4
        public SerializerStream UInt(ref uint value)
        {
            AppendType(PrimitiveType.UInt);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    Put(BitConverter.GetBytes(value));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToUInt32(Get(new byte[4]), 0);
                    break;
            }
            return this;
        }

        public SerializerStream Int(ref int value)
        {
            AppendType(PrimitiveType.Int);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    Put(BitConverter.GetBytes(value));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToInt32(Get(new byte[4]), 0);
                    break;
            }
            return this;
        }
        #endregion

        #region 8
        public SerializerStream ULong(ref ulong value)
        {
            AppendType(PrimitiveType.ULong);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    Put(BitConverter.GetBytes(value));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToUInt64(Get(new byte[8]), 0);
                    break;
            }
            return this;
        }

        public SerializerStream Long(ref long value)
        {
            AppendType(PrimitiveType.Long);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    Put(BitConverter.GetBytes(value));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToInt64(Get(new byte[8]), 0);
                    break;
            }
            return this;
        }
        #endregion
        #endregion

        #region Floats
        #region Single
        public SerializerStream Float(ref float value)
        {
            AppendType(PrimitiveType.Float);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    Put(BitConverter.GetBytes(value));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToSingle(Get(new byte[4]), 0);
                    break;
            }
            return this;
        }

        public SerializerStream IFloat16(ref float value, float resolution)
        {
            AppendType(PrimitiveType.IFloat16);
            AppendResolution(resolution);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    short v = (short) (value * resolution);
                    v -= value < 0 ? (short)1 : (short)0;
                    Put(BitConverter.GetBytes(v));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToInt16(Get(new byte[2]), 0);
                    value += value < 0 ? 1 : 0;
                    value /= resolution; 
                    break;
            }
            return this;
        }

        public SerializerStream IFloat8(ref float value, float resolution)
        {
            AppendType(PrimitiveType.IFloat8);
            AppendResolution(resolution);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    sbyte v = (sbyte)(value * resolution);
                    v -= value < 0 ? (sbyte)1 : (sbyte)0;
                    outer.WriteByte(unchecked((byte) v));
                    break;
                case SerializationMode.Deserialize:
                    value = unchecked((sbyte)outer.ReadByte());
                    value += value < 0 ? 1 : 0;
                    value /= resolution;
                    break;
            }
            return this;
        }
        #endregion

        #region Double
        public SerializerStream Double(ref double value)
        {
            AppendType(PrimitiveType.Double);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    Put(BitConverter.GetBytes(value));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToDouble(Get(new byte[8]), 0);
                    break;
            }
            return this;
        }
        
        public SerializerStream IDouble32(ref double value, double resolution)
        {
            AppendType(PrimitiveType.IDouble32);
            AppendResolution(resolution);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    int v = (int)(value * resolution);
                    v -= value < 0 ? (int)1 : (int)0;
                    Put(BitConverter.GetBytes(v));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToInt32(Get(new byte[4]), 0);
                    value += value < 0 ? 1 : 0;
                    value /= resolution;
                    break;
            }
            return this;
        }

        /*public SerializerStream IFloat16(ref float value, float resolution)
        {
            AppendType(PrimitiveType.IFloat16);
            AppendResolution(resolution);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    short v = (short)(value * resolution);
                    v -= value < 0 ? (short)1 : (short)0;
                    Put(BitConverter.GetBytes(v));
                    break;
                case SerializationMode.Deserialize:
                    value = BitConverter.ToInt16(Get(new byte[2]), 0) / resolution;
                    value += value < 0 ? 1 : 0;
                    break;
            }
            return this;
        }

        public SerializerStream IFloat8(ref float value, float resolution)
        {
            AppendType(PrimitiveType.IFloat16);
            AppendResolution(resolution);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    short v = (sbyte)(value * resolution);
                    v -= value < 0 ? (sbyte)1 : (sbyte)0;
                    Put(BitConverter.GetBytes(v));
                    break;
                case SerializationMode.Deserialize:
                    value = Get(new byte[1])[0] / resolution;
                    value += value < 0 ? 1 : 0;
                    break;
            }
            return this;
        }*/
        #endregion
        #endregion

        #region Text

        public SerializerStream String(ref string value)
        {
            AppendType(PrimitiveType.String);
            switch (currentMode)
            {
                case SerializationMode.Serialize:
                    if (value.Length > short.MaxValue) {
                        throw new ArgumentException("String to long");
                    }
                    var l1 = BitConverter.GetBytes((short)value.Length);
                    Put(l1);
                    var content = Encoding.ASCII.GetBytes(value);
                    outer.Write(content, 0, content.Length);
                    break;
                case SerializationMode.Deserialize:
                    //value = BitConverter.ToUInt16(Get(new byte[2]), 0);
                    var l2 = BitConverter.ToInt16(Get(new byte[2]), 0);
                    var buffer = new byte[l2];
                    var dummy = outer.Read(buffer, 0, l2) == l2 ? 0 : throw new EndOfStreamException();
                    value = Encoding.ASCII.GetString(buffer);
                    break;
            }
            return this;
        }

        #endregion
        #endregion

        public void AppendType(PrimitiveType type)
        {
            AppendHash((int)type);
        }

        public void AppendResolution(float resolution)
        {
            AppendHash(BitConverter.ToInt32(BitConverter.GetBytes(resolution), 0));
        }

        public void AppendResolution(double resolution)
        {
            var bytes = BitConverter.GetBytes(resolution);
            AppendHash(BitConverter.ToInt32(bytes, 0));
            AppendHash(BitConverter.ToInt32(bytes, 4));
        }

        private void AppendHash(int value)
        {
            SignatureHash *= 9176;
            SignatureHash += value;
        }

        private void Put(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            outer.Write(data, 0, data.Length);
        }

        private byte[] Get(byte[] buffer)
        {
            var l = buffer.Length;
            var dummy = outer.Read(buffer, 0, l) == l ? 0 : throw new EndOfStreamException();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);
            return buffer;
        }
    }

    public enum SerializationMode 
    {
        Serialize,
        Deserialize
    }

    public enum PrimitiveType : int
    {
        Invalid = 0,
        
        Byte = 1,
        SByte = 2,
        
        UShort = 3,
        Short = 4,
        
        UInt = 5,
        Int = 6,

        ULong = 7,
        Long = 8,

        Float = 9,
        Double = 10,
        
        IFloat8 = 11,
        IFloat16 = 12,

        IDouble8 = 13,
        IDouble16 = 14,
        IDouble32 = 15,
        
        Char = 16,
        String = 17
    }
}
