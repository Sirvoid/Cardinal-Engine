using System.Reflection;

namespace CardinalEngine {

    public class NetPlayer {
        // Queue for actions to be executed on the main thread
        private static Queue<Action> _mainThreadQueue = new();

        public Action<byte[]>? ServerSendData { get; set; }
        public Guid NetworkID { get; private set; }
        public Space? Space { get; internal set; }

        public delegate void DisconnectedHandler();
        public event DisconnectedHandler? OnDisconnected;

        private Dictionary<byte, Action> _packets = new();
        private NetReader _netReader = new();

        public NetPlayer() {
            _packets.Add(0, SendComponentCommand);
        }

        internal static void ReadMainThreadQueue() {
            while (_mainThreadQueue.Count > 0) {
                _mainThreadQueue.Dequeue()();
            }
        }

        internal void OnData(byte[] data) {
            _mainThreadQueue.Enqueue(() => {
                byte opcode = data[0];
                _netReader.Data = data;
                _netReader.Index = 1;
                Networking.Sender = this;
                _packets.GetValueOrDefault(opcode, () => { })();
            });
        }

        public void SendData(byte[] data) {
            ServerSendData?.Invoke(data);
        }

        internal void OnConnected(Guid networkID) {
            _mainThreadQueue.Enqueue(() => {
                NetworkID = networkID;
                SendData(Packet.SetNetworkId(this));
                Cardinal.Instance?.InvokePlayerConnected(this);
            });
        }

        internal void InvokeDisconnected() {
            _mainThreadQueue.Enqueue(() => {
                OnDisconnected?.Invoke();
                Cardinal.Instance?.InvokePlayerDisconnected(this);
            });
        }

        //********************//
        //** Packet Handlers **
        //*******************//

        private void SendComponentCommand() {
            int entityID = _netReader.ReadInt();
            byte componentID = _netReader.ReadByte();
            int commandID = _netReader.ReadByte();
            List<object> parameters = new List<object>();

            while (_netReader.Index < _netReader.Data.Length - 1) {
                parameters.Add(_netReader.ReadSupported());
            }

            NetEntity? entity = Space?.GetEntity(entityID);
            if (entity != null) {
                NetComponent? component = entity.GetComponent(componentID);
                if (component != null) {
                    MethodInfo? method = RemoteAttribute.GetMethodById(component, commandID);
                    method?.Invoke(component, parameters.ToArray());
                }
            }
        }
    }
}