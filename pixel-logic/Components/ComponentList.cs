using Raylib_cs;
namespace Game.Components
{
    // contains the mapping of basic types to int
    enum types
    {
        NONE,
        OUT,
        IN,
        CLKIN,
        CROSS,
        WIRE,
        AND,
        OR,
        XOR,
        NOT,
        BATTERY,
        CLK,
        FF,
        BUT,
        SEG,
    }

    // used to create components and store Custom Components
    public class ComponentList
    {
        public Dictionary<int, ConnectionCreator> connections = new Dictionary<int, ConnectionCreator>{
            {(int)types.IN, new ConnectionCreator(
                    "In",
                    new Color(179, 0, 0, 255), 
                    new Color(255, 0, 0, 255),
                    InConnection.newConnection
            )},
            {(int)types.OUT, new ConnectionCreator(
                    "Out",
                    new Color(61, 88, 143, 255), 
                    new Color(130, 154, 201, 255),
                    OutConnection.newConnection
            )},
            {(int)types.CLKIN, new ConnectionCreator(
                    "ClkIn",
                    new Color(179, 40, 40, 255), 
                    new Color(255, 40, 40, 255),
                    ClockIn.newConnection
            )},
            {(int)types.CROSS, new ConnectionCreator(
                    "Cross",
                    Color.GRAY, 
                    Color.GRAY, 
                    CrossConnection.newConnection
            )},
        };
        public Dictionary<int, ComponentCreator> basic = new Dictionary<int, ComponentCreator>{
            {(int)types.WIRE, new BasicComponentCreator(
                    "Wire",
                    new Color(153, 102, 51, 255), 
                    Color.YELLOW, 
                    WireComp.newComponent
            )},
            {(int)types.AND, new BasicComponentCreator(
                    "And",
                    new Color(230, 115, 0, 255), 
                    new Color(255, 153, 51, 255), 
                    AndComp.newComponent
            )},
            {(int)types.OR, new BasicComponentCreator(
                    "Or",
                    new Color(0, 0, 204, 255), 
                    new Color(26, 26, 255, 255), 
                    OrComp.newComponent
            )},
            {(int)types.XOR, new BasicComponentCreator(
                    "Xor",
                    new Color(89, 0, 179, 255), 
                    new Color(140, 26, 255, 255),
                    XorComp.newComponent
            )},
            {(int)types.NOT, new BasicComponentCreator(
                    "Not",
                    new Color(153, 0, 153, 255), 
                    new Color(255, 0, 255, 255), 
                    NotComp.newComponent
            )},
            {(int)types.BATTERY, new BasicComponentCreator(
                    "Battery",
                    new Color(255, 255, 255, 255), 
                    new Color(255, 255, 255, 255), 
                    BatteryComp.newComponent
            )},
            {(int)types.CLK, new BasicComponentCreator(
                    "Clock",
                    new Color(153, 153, 0, 255), 
                    new Color(204, 204, 51, 255), 
                    ClockComp.newComponent
            )},
            {(int)types.FF, new BasicComponentCreator(
                    "FlipFlop",
                    new Color(0, 122, 153, 255), 
                    new Color(0, 204, 255, 255), 
                    FlipFlopComp.newComponent
            )},
            {(int)types.BUT, new BasicComponentCreator(
                    "Button",
                    new Color(255, 140, 102, 255), 
                    new Color(255, 179, 102, 255), 
                    ButtonComp.newComponent
            )},
            {(int)types.SEG, new BasicComponentCreator(
                    "Display",
                    new Color(242, 242, 242, 255), 
                    new Color(242, 242, 242, 255), 
                    Seg7Comp.newComponent
            )},
        };

        public Dictionary<int, ComponentCreator> custom = new Dictionary<int, ComponentCreator>();

        public int Count
        {
            get {return basic.Count + custom.Count + connections.Count;}
        }

        public string getName(int i)
        {
            i++;
            if (connections.ContainsKey(i)) 
                return connections[i].name;
            else if (basic.ContainsKey(i)) 
                return basic[i].name;
            return custom[i].name;
        }

        private int findIndex(string name)
        {
            foreach (int i in custom.Keys)
            {
                if (custom[i].name == name)
                    return i;
            }
            return -1;
        }

        public int add(string filename)
        {
            string name = Path.GetFileNameWithoutExtension(filename);
            int index = findIndex(filename);
            if (index != -1) return index;
            index = Count+1;
            string ext = Path.GetExtension(filename);
            ComponentCreator c;
            switch (ext)
            {
                case ".json":
                    c = new Grid(filename);
                    break;
                default:
                    c = new CCode(filename);
                    break;
            }
            custom.Add(index, c);
            return index;
        }

        public Dictionary<int, string> toSave(Dictionary<int, bool> blocks)
        {
            Dictionary<int, string> names = new Dictionary<int, string>();
            foreach (int key in custom.Keys)
            {
                if (blocks.ContainsKey(key))
                    names[key] = custom[key].name;
            }
            return names;
        }

        public Component NewComponent(int type, State state)
        {
            if (basic.ContainsKey(type))
                return basic[type].createComponent(state);
            return custom[type].createComponent(state);
        }
        public Connection NewConnection(int type, Pos pos, State state)
        {
            return connections[type].createConnection(pos, state);
        }
        public void draw(int type, int x, int y, int gridsize, bool state)
        {
            if (connections.ContainsKey(type)) 
                connections[type].draw(x, y, gridsize, state);
            else if (basic.ContainsKey(type)) 
                basic[type].draw(x, y, gridsize, state);
            else
                custom[type].draw(x, y, gridsize, state);
        }
    }

    public abstract class CustomCreator
    {
        public string name = "";
        public Color offColor;
        public Color onColor;

        public virtual void draw(int x, int y, int gridsize, bool state) 
        {
            Color color = state? onColor: offColor;
            Raylib.DrawRectangle(x, y, gridsize, gridsize, color);
        }
    }
}