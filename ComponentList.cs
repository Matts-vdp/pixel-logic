using Raylib_cs;
namespace game
{
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

    class ComponentList
    {
        public List<string> items = new List<string>{ "Wire", "And", "Or", "Exor", "Not", "Out", "In", "ClkIn", "Cross", "Battery", "Clock", "Flip Flop", "Button", "Display" };
        public Dictionary<int, CustomComponent> components = new Dictionary<int, CustomComponent>();
        
        public static ComponentList fromDict(Dictionary<int,string> list){
            ComponentList cList = new ComponentList();
            foreach(int key in list.Keys) {
                cList.add(list[key]);
            }
            return cList;
        }
        public void add(string filename)
        {
            items.Add(Path.GetFileNameWithoutExtension(filename));
            string ext = Path.GetExtension(filename);
            CustomComponent c;
            switch (ext) {
                case ".json":
                    c = new Grid(filename);
                    break;
                default:
                    c = new CCode(filename);
                    break;
            }
            components.Add(items.Count, c);
        }
        public Dictionary<int,string> toSave(){
            Dictionary<int,string> names = new Dictionary<int, string>();
            foreach(int key in components.Keys) 
            {
                names[key] = components[key].name;
            }
            return names;
        }
        public Component NewComponent(int type)
        {
            switch ((types) type)
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
            switch ((types) type)
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

    abstract class CustomComponent
    {
        public abstract Component toComponent(ComponentList list, int type);
        public string name = "";

    }
}