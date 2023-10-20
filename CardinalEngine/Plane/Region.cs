using System.Numerics;

namespace CardinalEngine {

    public class Region {
        public const int SIZE = 16; // Constant X/Y/Z size for the region
        public Space Space { get; private set; }
        public List<NetEntity> Entities { get; private set; } = new(); // Entities within the region
        public List<Observer> Observers { get; private set; } = new(); // Region Observers
        public Vector3 GridPosition { get; private set; } // Position of the region within the space grid

        public event Action<Region>? OnRegionEmpty;

        public Region(Space space, Vector3 position) {
            GridPosition = position;
            Space = space;
        }

        public void AddEntity(NetEntity entity) {
            Entities.Add(entity);
            entity.CurrentRegion = this;
            NotifyObservers(entity, Packet.AddEntity, Packet.AddEntityComponent);
        }

        public void RemoveEntity(NetEntity entity) {
            Entities.Remove(entity);
            NotifyObservers(entity, Packet.RemoveEntity);
            if (Entities.Count == 0) OnRegionEmpty?.Invoke(this);
        }

        private void NotifyObserversOfTransfer(NetEntity entity, IEnumerable<Observer> oldObservers, IEnumerable<Observer> newObservers) {
            var leavingObservers = oldObservers.Except(newObservers).ToList(); // Observers who will no longer see the entity
            var enteringObservers = newObservers.Except(oldObservers).ToList(); // New observers who will now see the entity

            // Notify leaving observers about the entity removal
            leavingObservers.ForEach(viewer => viewer.NetPlayer?.SendData(Packet.RemoveEntity(entity)));

            // Notify entering observers about the new entity and its components
            enteringObservers.ForEach(viewer => {
                viewer.NetPlayer?.SendData(Packet.AddEntity(entity));
                NotifyObserverAboutEntityComponents(viewer, entity);
            });
        }

        // Notify viewers about an entity, with optional component notifications
        public void NotifyObservers(NetEntity entity, Func<NetEntity, byte[]> packetFunc, Func<NetEntity, NetComponent, byte[]>? componentPacketFunc = null) {
            var observers = GetObserversWithinRange(1).ToList();

            // Send entity packets to viewers
            observers.ForEach(observer => {
                observer.NetPlayer?.SendData(packetFunc(entity));
                if (componentPacketFunc == null) return;
                NotifyObserverAboutEntityComponents(observer, entity);
            });
        }

        public IEnumerable<NetEntity> GetEntitiesWithinRange(int regionRange) {
            return GetRegionsWithinRange(regionRange).SelectMany(region => region.Entities);
        }

        public IEnumerable<Observer> GetObserversWithinRange(int regionRange) {
            return GetRegionsWithinRange(regionRange).SelectMany(region => region.Observers);
        }

        public IEnumerable<Region> GetRegionsWithinRange(int regionRange) {
            for (int x = -regionRange; x <= regionRange; x++)
                for (int y = -regionRange; y <= regionRange; y++)
                    for (int z = -regionRange; z <= regionRange; z++)
                        foreach (var region in YieldNeighborRegion(x, y, z))
                            yield return region;
        }
        
        internal void TransferEntity(NetEntity entity, Region newRegion) {
            Region oldRegion = entity.CurrentRegion;

            oldRegion.Entities.Remove(entity);
            newRegion.Entities.Add(entity);
            entity.CurrentRegion = newRegion;

            var oldObservers = GetObserversWithinRange(1);
            var newObservers = newRegion.GetObserversWithinRange(1);
            NotifyObserversOfTransfer(entity, oldObservers, newObservers);

            entity.Transfer(oldRegion, newRegion);
        }

        private void NotifyObserverAboutEntityComponents(Observer observer, NetEntity entity) {
            foreach (NetComponent component in entity.GetComponents()) {
                if (LinkAttribute.GetTypeID(component) != ushort.MaxValue) {
                    observer.NetPlayer?.SendData(Packet.AddEntityComponent(entity, component));
                    component.OnDataRequest(observer);
                }
            }
        }

        private IEnumerable<Region> YieldNeighborRegion(int x, int y, int z) {
            Vector3 neighborPosition = GridPosition + new Vector3(x, y, z);
            Region? neighborRegion = Space.GetRegionByGridPosition(neighborPosition);
            if (neighborRegion != null) {
                yield return neighborRegion;
            }
        }
    }
}