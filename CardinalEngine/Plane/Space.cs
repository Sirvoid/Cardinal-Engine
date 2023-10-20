using System.Numerics;

namespace CardinalEngine {

    public class Space {
        public Dictionary<Vector3, Region> Regions { get; private set; }
        public Dictionary<int, NetEntity> Entities { get; private set; }
        public List<Observer> Observers { get; private set; }
        public Cardinal Cardinal { get; private set; }

        public Space(Cardinal cardinal) {
            Regions = new Dictionary<Vector3, Region>();
            Entities = new Dictionary<int, NetEntity>();
            Observers = new List<Observer>();
            Cardinal = cardinal;
        }

        public Region? GetRegionByGridPosition(Vector3 gridPosition) {
            return Regions.TryGetValue(gridPosition, out var region) ? region : null;
        }

        public Region RetrieveRegionByEntityPosition(Vector3 entityPosition) {
            Vector3 gridPosition = new Vector3(
                (float)Math.Floor(entityPosition.X / Region.SIZE),
                (float)Math.Floor(entityPosition.Y / Region.SIZE),
                (float)Math.Floor(entityPosition.Z / Region.SIZE)
            );

            Region? region = GetRegionByGridPosition(gridPosition);
            if (region != null) return region;

            return CreateRegionAt(gridPosition);
        }

        internal void RemoveRegion(Region region) {
            Regions.Remove(region.GridPosition);
        }

        internal Region CreateRegionAt(Vector3 gridPosition) {
            Region newRegion = new Region(this, gridPosition);
            newRegion.OnRegionEmpty += RemoveRegion;
            Regions.Add(gridPosition, newRegion);
            return newRegion;
        }

        internal void AddEntity(int ID, NetEntity entity) {
            Entities.Add(ID, entity);
        }

        internal void RemoveEntity(int ID) {
            Entities.Remove(ID);
        }

        internal NetEntity? GetEntity(int ID) {
            if (Entities.ContainsKey(ID)) return Entities[ID];
            return null;
        }
    }
}