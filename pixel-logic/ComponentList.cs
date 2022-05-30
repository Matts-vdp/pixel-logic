using Raylib_cs;
namespace game
{
    // contains the mapping of basic types to int
    enum types
    {
        NONE,
        WIRE,
        AND,
        OR,
        XOR,
        NOT,
        OUT,
        IN,
        CLKIN,
        CROSS,
        BATTERY,
        CLK,
        FF,
        BUT,
        SEG,
    }

    // used to create components and store Custom Components
    public class ComponentList
    {
        public List<string> items = new List<string> { "Wire", "And", "Or", "Exor", "Not", "Out", "In", "ClkIn", "Cross", "Battery", "Clock", "Flip Flop", "Button", "Display" };
        public Dictionary<int, CustomComponentCreator> components = new Dictionary<int, CustomComponentCreator>();

        public int add(string filename)
        {
            string name = Path.GetFileNameWithoutExtension(filename);
            int index = items.IndexOf(name);
            if (index != -1) return index + 1;
            items.Add(name);
            string ext = Path.GetExtension(filename);
            CustomComponentCreator c;
            switch (ext)
            {
                case ".json":
                    c = new Grid(filename);
                    break;
                default:
                    c = new CCode(filename);
                    break;
            }
            components.Add(items.Count, c);
            return items.Count;
        }

        public Dictionary<int, string> toSave(Dictionary<int, bool> blocks)
        {
            Dictionary<int, string> names = new Dictionary<int, string>();
            foreach (int key in components.Keys)
            {
                if (blocks.ContainsKey(key))
                    names[key] = components[key].name;
            }
            return names;
        }
        public Component NewComponent(int type)
        {
            switch ((types)type)
            {
                case (types.WIRE):
                    return new WireComp(this);
                case (types.BATTERY):
                    return new BatComp(this);
                case (types.AND):
                    return new AndComp(this);
                case (types.CLK):
                    return new Clock(this);
                case (types.NOT):
                    return new NotComp(this);
                case (types.OR):
                    return new OrComp(this);
                case (types.FF):
                    return new FlipFlop(this);
                case (types.BUT):
                    return new Button(this);
                case (types.XOR):
                    return new XorComp(this);
                case (types.SEG):
                    return new Seg7(this);
                default:
                    return components[type].toComponent(this, type);
            }
        }
        public Connection NewConnection(int type, Pos pos)
        {
            switch ((types)type)
            {
                case (types.OUT):
                    return new OutConnection(pos);
                case (types.CLKIN):
                    return new ClockIn(pos);
                case (types.CROSS):
                    return new CrossConnection(pos);
                default:
                    return new InConnection(pos);
            }
        }
    }

    // represents a class that can be used as a Custom Component
    public abstract class CustomComponentCreator
    {
        public abstract Component toComponent(ComponentList list, int type);
        public string name = "";

    }
}