using System.Numerics;
using System.Text;

namespace CardinalEngine {

    internal class NetWriter {
        public int Index { get; set; }
        public byte[] Data { get; set; }
        private Dictionary<Type, Action<object>> _writersByType = new Dictionary<Type, Action<object>>();
        private Dictionary<Type, byte> _typeIds = new Dictionary<Type, byte>();

        public NetWriter() {
            Data = new byte[0];
            _writersByType = new Dictionary<Type, Action<object>> {
            { typeof(byte), obj => WriteByte((byte)obj) },
            { typeof(sbyte), obj => WriteSByte((sbyte)obj) },
            { typeof(short), obj => WriteShort((short)obj) },
            { typeof(ushort), obj => WriteUShort((ushort)obj) },
            { typeof(int), obj => WriteInt((int)obj) },
            { typeof(float), obj => WriteFloat((float)obj) },
            { typeof(double), obj => WriteDouble((double)obj) },
            { typeof(long), obj => WriteLong((long)obj) },
            { typeof(ulong), obj => WriteULong((ulong)obj) },
            { typeof(Vector3), obj => WriteVector3((Vector3)obj) },
            { typeof(string), obj => WriteString((string)obj) },
            { typeof(Guid), obj => WriteGuid((Guid)obj) }
        };

            _typeIds = new Dictionary<Type, byte>
            {
            { typeof(byte), 0 },
            { typeof(sbyte), 1 },
            { typeof(short), 2 },
            { typeof(ushort), 3 },
            { typeof(int), 4 },
            { typeof(float), 5 },
            { typeof(double), 6 },
            { typeof(long), 7 },
            { typeof(ulong), 8 },
            { typeof(Vector3), 9 },
            { typeof(string), 10 },
            { typeof(Guid), 11 }
        };
        }

        public void NewPacket(int packetSize) {
            Index = 0;
            Data = new byte[packetSize];
        }

        public void WriteByte(byte value) {
            Data[Index] = value;
            Index += 1;
        }

        public void WriteSByte(sbyte value) {
            WriteByte((byte)value);
        }

        public void WriteShort(short value) {
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteUShort(ushort value) {
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteInt(int value) {
            WriteByte((byte)(value >> 24));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)(value));
        }

        public void WriteFloat(float value) {
            WriteInt(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }

        public void WriteDouble(double value) {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) {
                WriteByte(b);
            }
        }

        public void WriteLong(long value) {
            for (int i = 7; i >= 0; i--) {
                WriteByte((byte)(value >> (8 * i)));
            }
        }

        public void WriteULong(ulong value) {
            for (int i = 7; i >= 0; i--) {
                WriteByte((byte)(value >> (8 * i)));
            }
        }

        public void WriteVector3(Vector3 vector3) {
            WriteFloat(vector3.X);
            WriteFloat(vector3.Y);
            WriteFloat(vector3.Z);
        }

        public void WriteString(string value) {
            ushort maxLength = ushort.MaxValue;
            byte[] stringBytes = Encoding.UTF8.GetBytes(value);

            if (stringBytes.Length > maxLength) {
                throw new ArgumentException($"String is too long to write. Maximum length is {maxLength} bytes.");
            }

            WriteUShort((ushort)stringBytes.Length);

            foreach (byte b in stringBytes) {
                WriteByte(b);
            }
        }

        private void WriteGuid(Guid obj) {
            byte[] bytes = obj.ToByteArray();
            Array.Copy(bytes, 0, Data, Index, bytes.Length);
            Index += bytes.Length;
        }

        public void WriteByteArray(byte[] bytes) {
            WriteByte((byte)bytes.Length);
            Array.Copy(bytes, 0, Data, Index, bytes.Length);
            Index += bytes.Length;
        }

        public void WriteSupported(Type variableType, object? value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value), "NetWriter Error: Cannot write a null value!");
            }

            if (_writersByType.TryGetValue(variableType, out var writerAction)) {
                WriteByte(_typeIds[variableType]);
                writerAction(value);
            } else {
                throw new InvalidOperationException($"NetWriter Error: Unsupported type {variableType}!");
            }
        }
    }
}