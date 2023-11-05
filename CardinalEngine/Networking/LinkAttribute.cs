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
            throw new Exception("Component has no link.");
        }

        internal static void CompileComponentTypesID() {
            List<Assembly> assemblies = new List<Assembly> { Assembly.GetExecutingAssembly()};
            Assembly? entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null) assemblies.Add(entryAssembly);

            foreach(Assembly assembly in assemblies) { 
                Type[] types = assembly.GetTypes();
            
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
}