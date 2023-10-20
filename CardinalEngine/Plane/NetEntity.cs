using System.Numerics;

namespace CardinalEngine {

    public class NetEntity {
        private static int IDCounter = 0;
        public int ID { get; private set; }
        public Vector3 Position { get; private set; }
        public Region CurrentRegion { get; internal set; }
        public Space Space { get; private set; }
        private Dictionary<byte, NetComponent> _components = new();

        public NetEntity(Space space, Vector3 position) {
            ID = IDCounter++;
            Position = position;
            Space = space;

            Region currentRegion = Space.RetrieveRegionByEntityPosition(position);
            currentRegion.AddEntity(this);
            CurrentRegion = currentRegion;

            Space.AddEntity(ID, this);
        }

        public void Destroy() {
            CurrentRegion.RemoveEntity(this);
            Space.RemoveEntity(ID);
        }

        public void Move(Vector3 dir) {
            MoveTo(Position + dir);
        }

        public void MoveTo(Vector3 newPosition) {
            Position = newPosition;
            Region oldRegion = CurrentRegion;
            Region newRegion = Space.RetrieveRegionByEntityPosition(newPosition);

            if (newRegion != oldRegion) {
                oldRegion.TransferEntity(this, newRegion);
            } else {
                oldRegion.NotifyObservers(this, Packet.UpdateEntityPosition);
            }
        }

        public T AddComponent<T>() where T : NetComponent, new() {

            byte b = GetFreeComponentID();
            T component = new();
            component.InitComponent(this, b);

            _components.Add(b, component);

            var observers = CurrentRegion.GetObserversWithinRange(1);
            foreach (Observer observer in observers) {
                if (LinkAttribute.GetTypeID(component) != ushort.MaxValue) {
                    observer.NetPlayer?.SendData(Packet.AddEntityComponent(this, component));
                    component.OnDataRequest(observer);
                }
            }

            component.OnAdded();

            return component;
        }

        public NetComponent? GetComponent(byte componentID) {
            if (_components.ContainsKey(componentID)) {
                return _components[componentID];
            } else {
                return null;
            }
        }

        public T? GetComponent<T>() where T : NetComponent {
            foreach (var component in _components.Values) {
                if (component is T typedComponent) {
                    return typedComponent;
                }
            }
            return default;
        }

        public void RemoveComponent<T>() where T : NetComponent {
            byte? keyToRemove = null;
            foreach (var (key, component) in _components) {
                if (component is T) {
                    keyToRemove = key;
                    break;
                }
            }

            if (keyToRemove.HasValue) {
                var observers = CurrentRegion.GetObserversWithinRange(1);
                foreach (Observer observer in observers) {
                    observer.NetPlayer?.SendData(Packet.RemoveEntityComponent(this, keyToRemove.Value));
                }
                _components.Remove(keyToRemove.Value);
            }
        }

        public NetComponent[] GetComponents() {
            return _components.Values.ToArray();
        }

        internal void Transfer(Region oldRegion, Region newRegion) {
            CurrentRegion = newRegion;
            foreach (NetComponent component in _components.Values) {
                component.OnTransfer(oldRegion, newRegion);
            }
        }

        private byte GetFreeComponentID() {
            for (byte b = 0; b < 255; b++) {
                if (!_components.ContainsKey(b)) {
                    return b;
                }
            }
            throw new InvalidOperationException("All component IDs are in use.");
        }
    }
}