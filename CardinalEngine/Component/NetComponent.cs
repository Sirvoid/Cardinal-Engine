﻿namespace CardinalEngine {

    public class NetComponent {
        public NetEntity Entity { get; private set; } = null!;
        public byte ID { get; private set; }

        internal void InitComponent(NetEntity entity, byte componentID) {
            Entity = entity;
            ID = componentID;
        }

        public virtual void Destroy() {
        }

        public virtual void OnTransfer(Region oldRegion, Region newRegion) {
        }

        public virtual void OnAdded() {
        }

        public virtual void OnDataRequest(Observer requester) {
        }

        public void Synchronize(int fieldID, NetPlayer netPlayer) {
            SyncAttribute.Process(this, fieldID, netPlayer);
        }

        public void Synchronize(int fieldID) {
            SyncAttribute.Process(this, fieldID);
        }

        public void SynchronizeAll() {
            SyncAttribute.ProcessAll(this);
        }
    }
}