using System.Reflection;

namespace CardinalEngine {

    [AttributeUsage(AttributeTargets.Field)]
    internal class SyncAttribute : Attribute {
        private int _fieldID;

        public SyncAttribute(int fieldID) {
            _fieldID = fieldID;
        }

        internal static void Process(NetComponent component, int fieldID, NetPlayer? player = null) {
            var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            NetEntity? entity = component.Entity;

            if (entity == null) {
                throw new Exception("Component is not attached to an entity.");
            }

            foreach (var field in fields) {
                var attributes = field.GetCustomAttributes(typeof(SyncAttribute), true);

                if (attributes.Length > 0) {
                    Type variableType = field.FieldType;

                    foreach (var attribute in attributes) {
                        SyncAttribute syncAttribute = (SyncAttribute)attribute;
                        if (syncAttribute._fieldID != fieldID) continue;

                        if (player != null) {
                            player.SendData(Packet.SyncComponentField(entity, component, syncAttribute._fieldID, variableType, field.GetValue(component)));
                            return;
                        }

                        foreach (var viewer in entity.CurrentRegion.GetObserversWithinRange(1)) {
                            viewer.NetPlayer?.SendData(Packet.SyncComponentField(entity, component, syncAttribute._fieldID, variableType, field.GetValue(component)));
                            return;
                        }
                    }

                    break;
                }
            }
        }

        internal static void ProcessAll(NetComponent component) {
            var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            NetEntity? entity = component.Entity;

            if (entity == null) {
                throw new Exception("Component is not attached to an entity.");
            }

            foreach (var field in fields) {
                var attributes = field.GetCustomAttributes(typeof(SyncAttribute), true);
                if (attributes.Length > 0) {
                    Type variableType = field.FieldType;
                    foreach (var attribute in attributes) {
                        SyncAttribute syncAttribute = (SyncAttribute)attribute;
                        foreach (var viewer in entity.CurrentRegion.GetObserversWithinRange(1)) {
                            viewer.NetPlayer?.SendData(Packet.SyncComponentField(entity, component, syncAttribute._fieldID, variableType, field.GetValue(component)));
                        }
                    }
                }
            }
        }
    }
}