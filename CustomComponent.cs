namespace game
{
    class ComponentList
    {
        public static Dictionary<int, CustomComponent> components = new Dictionary<int, CustomComponent>();
        public static void add(string filename, CustomComponent component)
        {
            ComponentFactory.items.Add(Path.GetFileNameWithoutExtension(filename));
            ComponentList.components.Add(ComponentFactory.items.Count, component);
        }
    }
    abstract class CustomComponent
    {
        public abstract Component toComponent(int type);
        public string name = "c";

    }
}