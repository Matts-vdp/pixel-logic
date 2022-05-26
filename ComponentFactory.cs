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

    static class ComponentFactory
    {
        public static List<string> items = new List<string>{ "Wire", "And", "Or", "Exor", "Not", "Out", "In", "ClkIn", "Cross", "Battery", "Clock", "Flip Flop", "Button", "Display" };

        public static Component NewComponent(int type)
        {
            switch ((types) type)
            {
                case (types.WIRE):
                    return new WireComp();
                case (types.BATTERY):
                    return new BatComp();
                case (types.AND):
                    return new AndComp();
                case (types.CLK):
                    return new Clock();
                case (types.NOT):
                    return new NotComp();
                case (types.OR):
                    return new OrComp();
                case (types.FF):
                    return new FlipFlop();
                case (types.BUT):
                    return new Button();
                case (types.XOR):
                    return new XorComp();
                case (types.SEG):
                    return new Seg7();
                default:
                    return ComponentList.components[type].toComponent(type);
            }
        }
        public static Connection NewConnection(int type, Pos pos)
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
}