using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CardinalEngine;

namespace Example {
    [Link(0)]
    internal class TestComponent: NetComponent {

        [Sync(0)]
        public Vector3 Rotation;

        [Sync(1)]
        public int Health;

        [Sync(2)]
        public string Username = "";

        public override void OnAdded() {
            Rotation = new Vector3(90, 30, 20);
            Health = 100;
            Username = "test";

            if (Cardinal.Instance != null) {
                Cardinal.Instance.OnUpdate += OnUpdate;
            }
        }

        private void OnUpdate() {
            SynchronizeAll();
        }

        [Remote(0)]
        public void RemoteMove(Vector3 direction) {
            Observer? viewer = Entity.GetComponent<Observer>();
            if (viewer != null) {
                if (viewer.NetPlayer == Networking.Sender) {
                    Entity.Move(direction);
               }
            }
        }

    }
}
