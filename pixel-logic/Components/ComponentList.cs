using Raylib_cs;
namespace Game.Components
{
    // contains the mapping of basic types to int
    public enum Types
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
        public Dictionary<int, ConnectionCreator> Connections = new()
        {
            {
                (int)Types.IN,
                new ConnectionCreator(
                    "In",
                    new Color(179, 0, 0, 255),
                    new Color(255, 0, 0, 255),
                    InConnection.NewConnection
            )
            },
            {
                (int)Types.OUT,
                new ConnectionCreator(
                    "Out",
                    new Color(61, 88, 143, 255),
                    new Color(130, 154, 201, 255),
                    OutConnection.NewConnection
            )
            },
            {
                (int)Types.CLKIN,
                new ConnectionCreator(
                    "ClkIn",
                    new Color(179, 40, 40, 255),
                    new Color(255, 40, 40, 255),
                    ClockIn.NewConnection
            )
            },
            {
                (int)Types.CROSS,
                new ConnectionCreator(
                    "Cross",
                    Color.GRAY,
                    Color.GRAY,
                    CrossConnection.NewConnection
            )
            },
        };
        public Dictionary<int, ComponentCreator> Basic = new()
        {
            {
                (int)Types.WIRE,
                new BasicComponentCreator(
                    "Wire",
                    new Color(163, 120, 60, 255),
                    Color.YELLOW,
                    WireComp.NewComponent
            )
            },
            {
                (int)Types.AND,
                new BasicComponentCreator(
                    "And",
                    new Color(230, 115, 0, 255),
                    new Color(255, 153, 51, 255),
                    AndComp.NewComponent
            )
            },
            {
                (int)Types.OR,
                new BasicComponentCreator(
                    "Or",
                    new Color(0, 0, 204, 255),
                    new Color(26, 26, 255, 255),
                    OrComp.NewComponent
            )
            },
            {
                (int)Types.XOR,
                new BasicComponentCreator(
                    "Xor",
                    new Color(89, 0, 179, 255),
                    new Color(140, 26, 255, 255),
                    XorComp.NewComponent
            )
            },
            {
                (int)Types.NOT,
                new BasicComponentCreator(
                    "Not",
                    new Color(153, 0, 153, 255),
                    new Color(255, 0, 255, 255),
                    NotComp.NewComponent
            )
            },
            {
                (int)Types.BATTERY,
                new BasicComponentCreator(
                    "Battery",
                    new Color(255, 255, 255, 255),
                    new Color(255, 255, 255, 255),
                    BatteryComp.NewComponent
            )
            },
            {
                (int)Types.CLK,
                new BasicComponentCreator(
                    "Clock",
                    new Color(153, 153, 0, 255),
                    new Color(204, 204, 51, 255),
                    ClockComp.NewComponent
            )
            },
            {
                (int)Types.FF,
                new BasicComponentCreator(
                    "FlipFlop",
                    new Color(0, 122, 153, 255),
                    new Color(0, 204, 255, 255),
                    FlipFlopComp.NewComponent
            )
            },
            {
                (int)Types.BUT,
                new BasicComponentCreator(
                    "Button",
                    new Color(255, 140, 102, 255),
                    new Color(255, 179, 102, 255),
                    ButtonComp.NewComponent
            )
            },
            {
                (int)Types.SEG,
                new BasicComponentCreator(
                    "Display",
                    new Color(242, 242, 242, 255),
                    new Color(242, 242, 242, 255),
                    Seg7Comp.NewComponent
            )
            },
        };

        public Dictionary<int, ComponentCreator> Custom = new();

        private readonly IFile _file;

        public ComponentList(IFile file)
        {
            _file = file;
        }

        public int Count
        {
            get { return Basic.Count + Custom.Count + Connections.Count; }
        }

        public string GetName(int i)
        {
            if (Connections.ContainsKey(i))
                return Connections[i].Name;
            else if (Basic.ContainsKey(i))
                return Basic[i].Name;
            return Custom[i].Name;
        }

        private int FindIndex(string name)
        {
            foreach (int i in Custom.Keys)
            {
                if (Custom[i].Name == name)
                    return i;
            }
            return -1;
        }

        public int Add(string filename)
        {
            int index = FindIndex(filename);
            if (index != -1) return index;
            index = Count + 1;
            string ext = Path.GetExtension(filename);
            string txt = _file.ReadAllText(filename);
            ComponentCreator c;
            if (ext == ".json")
                c = new Field(filename, txt, _file);
            else
                c = new CCode(filename, txt);
            Custom.Add(index, c);
            return index;
        }



        public Component NewComponent(int type, State state)
        {
            if (Basic.ContainsKey(type))
                return Basic[type].CreateComponent(state);
            return Custom[type].CreateComponent(state);
        }
        public Connection NewConnection(int type, Pos pos, State state)
        {
            return Connections[type].CreateConnection(pos, state);
        }
        public void Draw(int type, int x, int y, int gridsize, bool state)
        {
            if (Connections.ContainsKey(type))
                Connections[type].Draw(x, y, gridsize, state);
            else if (Basic.ContainsKey(type))
                Basic[type].Draw(x, y, gridsize, state);
            else
                Custom[type].Draw(x, y, gridsize, state);
        }
    }

    public abstract class CustomCreator
    {
        public string Name = "";
        public Color OffColor;
        public Color OnColor;

        public virtual void Draw(int x, int y, int gridsize, bool state)
        {
            Color color = state ? OnColor : OffColor;
            Raylib.DrawRectangle(x, y, gridsize, gridsize, color);
        }
    }
}