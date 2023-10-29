using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace CardinalEngine {

    public class Packet {
        private static NetWriter _writer = new NetWriter();

        public static byte[] AddEntity(NetEntity entity) {
            _writer.NewPacket(17);
            _writer.WriteByte((byte)Opcode.AddEntity);
            _writer.WriteInt(entity.ID);
            _writer.WriteVector3(entity.Position);
            return _writer.Data;
        }

        public static byte[] RemoveEntity(NetEntity entity) {
            _writer.NewPacket(5);
            _writer.WriteByte((byte)Opcode.RemoveEntity);
            _writer.WriteInt(entity.ID);
            return _writer.Data;
        }

        public static byte[] UpdateEntityPosition(NetEntity entity) {
            _writer.NewPacket(17);
            _writer.WriteByte((byte)Opcode.UpdateEntityPosition);
            _writer.WriteInt(entity.ID);
            _writer.WriteVector3(entity.Position);
            return _writer.Data;
        }

        public static byte[] SyncComponentField(NetEntity entity, NetComponent component, int _fieldID, Type variableType, object? v) {
            if (v == null) {
                throw new ArgumentNullException(nameof(v), "Value cannot be null.");
            }

            int dataSize = 0;

            if (variableType == typeof(string))
                dataSize = Encoding.UTF8.GetByteCount((string)v) + 2;
            else
                dataSize = Marshal.SizeOf(variableType);

            _writer.NewPacket(7 + dataSize + 1);
            _writer.WriteByte((byte)Opcode.SyncEntityField);
            _writer.WriteInt(entity.ID);
            _writer.WriteByte(component.ID);
            _writer.WriteByte((byte)_fieldID);
            _writer.WriteSupported(variableType, v);
            return _writer.Data;
        }

        public static byte[] AddEntityComponent(NetEntity entity, NetComponent component) {
            _writer.NewPacket(8);
            _writer.WriteByte((byte)Opcode.AddEntityComponent);
            _writer.WriteInt(entity.ID);
            _writer.WriteByte(component.ID);
            _writer.WriteUShort(LinkAttribute.GetTypeID(component));
            return _writer.Data;
        }

        public static byte[] RemoveEntityComponent(NetEntity entity, byte componentID) {
            _writer.NewPacket(6);
            _writer.WriteByte((byte)Opcode.RemoveEntityComponent);
            _writer.WriteInt(entity.ID);
            _writer.WriteByte(componentID);
            return _writer.Data;
        }

        public static byte[] SetNetworkId(NetPlayer player) {
            _writer.NewPacket(18);
            _writer.WriteByte((byte)Opcode.SetNetworkId);
            _writer.WriteByteArray(player.NetworkID.ToByteArray());
            return _writer.Data;
        }

        internal static byte[] SendComponentCommand(NetComponent component, byte commandID, object[] parameters) {
            int packetSize = 7;

            foreach (var param in parameters) {
                if (param == null) continue;
                Type type = param.GetType();
                if (type == typeof(string)) {
                    packetSize += 1 + 2 + Encoding.UTF8.GetByteCount((string)param);
                } else {
                    packetSize += 1 + Marshal.SizeOf(type);
                }
            }

            _writer.NewPacket(packetSize);
            _writer.WriteByte((byte)Opcode.SendCommand);
            _writer.WriteInt(component.Entity.ID);
            _writer.WriteByte(component.ID);
            _writer.WriteByte(commandID);
            foreach (var param in parameters) {
                if (param == null) continue;
                _writer.WriteSupported(param.GetType(), param);
            }
            return _writer.Data;
        }
    }
}