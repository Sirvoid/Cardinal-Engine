namespace CardinalEngine {

    [Link(ushort.MaxValue - 1)]
    public class Observer : NetComponent {

        [Sync(0)]
        public Guid NetworkID;

        public NetPlayer? NetPlayer { get; private set; }
        public bool DestroyEntityOnDisconnect { get; private set; }

        public void Link(NetPlayer netPlayer, bool destroyEntityOnDisconnect = false) {
            NetPlayer = netPlayer;
            DestroyEntityOnDisconnect = destroyEntityOnDisconnect;
            NetPlayer.OnDisconnected += OnDisconnect;
            NetworkID = netPlayer.NetworkID;
            Synchronize(0);
            SendEntitiesData();
            UpdateViewersList(Entity.CurrentRegion, addAction: true);
            ValidatePlayerSpace(Entity);
        }

        public override void Destroy() {
            base.Destroy();
            UpdateViewersList(Entity.CurrentRegion, addAction: false);
        }

        public override void OnTransfer(Region oldRegion, Region newRegion) {
            base.OnTransfer(oldRegion, newRegion);
            TransferViewers(oldRegion, newRegion);
            SendDataOnTransfer(oldRegion, newRegion);
        }

        public override void OnDataRequest(Observer requester) {
            if(requester.NetPlayer != null)
                Synchronize(0, requester.NetPlayer);
        }

        public void OnDisconnect() {
            if (DestroyEntityOnDisconnect) {
                Entity.Destroy();
            } else {
                Entity.RemoveComponent<Observer>();
            }
        }
        private void ValidatePlayerSpace(NetEntity entity) {
            if (NetPlayer == null) return;

            if (NetPlayer.Space == null || NetPlayer.Space == entity.CurrentRegion.Space) {
                NetPlayer.Space = entity.CurrentRegion.Space;
            } else {
                throw new Exception("NetPlayer can't be in two different spaces.");
            }
        }

        private void UpdateViewersList(Region? region, bool addAction) {
            if (region == null) return;

            if (addAction) {
                region.Observers.Add(this);
                region.Space.Observers.Add(this);
            } else {
                region.Observers.Remove(this);
                region.Space.Observers.Remove(this);
            }
        }

        private void TransferViewers(Region oldRegion, Region newRegion) {
            UpdateViewersList(oldRegion, addAction: false);
            UpdateViewersList(newRegion, addAction: true);
        }

        private void SendDataOnTransfer(Region oldRegion, Region newRegion) {
            var newEntities = newRegion.GetEntitiesWithinRange(1);
            var oldEntities = oldRegion.GetEntitiesWithinRange(1);

            foreach (NetEntity entity in newEntities.Except(oldEntities)) {
                SendEntityData(entity);
            }

            foreach (NetEntity entity in oldEntities.Except(newEntities)) {
                NetPlayer?.SendData(Packet.RemoveEntity(entity));
            }
        }

        //Send Entities to this player
        private void SendEntitiesData() {
            var entities = Entity.CurrentRegion.GetEntitiesWithinRange(1);
            if (entities == null) return;

            foreach (NetEntity entity in entities) {
                SendEntityData(entity);
            }
        }

        private void SendEntityData(NetEntity entity) {
            NetPlayer?.SendData(Packet.AddEntity(entity));
            foreach (NetComponent component in entity.GetComponents()) {
                if (LinkAttribute.GetTypeID(component) == ushort.MaxValue) continue;

                NetPlayer?.SendData(Packet.AddEntityComponent(entity, component));
                component.OnDataRequest(this);
            }
        }
    }
}