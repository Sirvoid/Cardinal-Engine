using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Timers;

namespace CardinalEngine {
    public class Cardinal {
        public static Cardinal? Instance;
        public List<Space> Spaces { get; private set; } = new List<Space>();
        
        public delegate void CardinalUpdateHandler();
        public event CardinalUpdateHandler? OnUpdate;

        public delegate void CardinalPlayerConnectedHandler(NetPlayer netPlayerHandler);
        public event CardinalPlayerConnectedHandler? OnPlayerConnected;

        public delegate void CardinalPlayerDisconnectedHandler(NetPlayer netPlayerHandler);
        public event CardinalPlayerDisconnectedHandler? OnPlayerDisconnected;

        private PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(1.0f / 60));

        public Cardinal() { 
            Instance = this;
            LinkAttribute.CompileComponentTypesID();
        }

        public void Start() {
            WServer.Run();
            DoUpdates();
        }

        public Space AddSpace() {
            Space space = new Space(this);
            Spaces.Add(space);
            return space;
        }

        internal void InvokePlayerConnected(NetPlayer netPlayerHandler) {
            OnPlayerConnected?.Invoke(netPlayerHandler);
        }

        internal void InvokePlayerDisconnected(NetPlayer netPlayerHandler) {
            OnPlayerDisconnected?.Invoke(netPlayerHandler);
        }

        private async void DoUpdates() {
            while (await _timer.WaitForNextTickAsync()) {
                NetPlayer.ReadMainThreadQueue();
                OnUpdate?.Invoke();
            }
        }

    }
}
