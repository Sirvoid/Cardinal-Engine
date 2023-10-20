using System.Reflection;

namespace CardinalEngine {

    [AttributeUsage(AttributeTargets.Class)]
    public class LinkAttribute : Attribute {
        internal static Dictionary<Type, ushort> TypesID = new();

        private int _componentTypeID;

        public LinkAttribute(int componentTypeID) {
            _componentTypeID = componentTypeID;
        }

        public static ushort GetTypeID(NetComponent component) {
            Type type = component.GetType();
            if (TypesID.ContainsKey(type)) return TypesID[type];
            return ushort.MaxValue;
        }

        internal static void CompileComponentTypesID() {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in types) {
                if (type.IsSubclassOf(typeof(NetComponent))) {
                    object[] attributes = type.GetCustomAttributes(typeof(LinkAttribute), false);

                    foreach (LinkAttribute attribute in attributes) {
                        TypesID[type] = (ushort)attribute._componentTypeID;
                    }
                }
            }
        }
    }
}