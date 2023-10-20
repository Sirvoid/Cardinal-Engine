using NetCoreServer;
using System.Net;
using System.Net.Sockets;

namespace CardinalEngine {

    internal class WSession : WsSession {
        private NetPlayer _networkHandler = new NetPlayer();

        internal WSession(WsServer server) : base(server) {
        }

        public override void OnWsConnected(HttpRequest request) {
            Console.WriteLine($"WebSocket session with Id {Id} connected!");

            _networkHandler.ServerSendData = SendData;
            _networkHandler.OnConnected(Id);
        }

        public override void OnWsDisconnected() {
            Console.WriteLine($"WebSocket session with Id {Id} disconnected!");
            _networkHandler.InvokeDisconnected();
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size) {
            byte[] data = new byte[size];
            Array.Copy(buffer, offset, data, 0, size);
            _networkHandler.OnData(data);
        }

        public void SendData(byte[] data) {
            SendBinaryAsync(data);
        }

        protected override void OnError(SocketError error) {
            Console.WriteLine($"WebSocket session caught an error with code {error}");
        }
    }

    internal class WServer : WsServer {
        private static WServer _server = new WServer(IPAddress.Any, 8080);

        internal static void Run() {
            Console.Write("Server starting...");
            _server.Start();
        }

        public WServer(IPAddress address, int port) : base(address, port) {
        }

        protected override TcpSession CreateSession() {
            return new WSession(this);
        }

        protected override void OnError(SocketError error) {
            Console.WriteLine($"WebSocket server caught an error with code {error}");
        }
    }
}