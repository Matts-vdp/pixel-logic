
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
        CROSS,
        BATTERY,
        CLK,
        FF,
        BUT,
        SEG,
    }

    static class ComponentFactory
    {
        public static String[] items = { "Wire", "And", "Or", "Exor", "Not", "Out", "In", "Cross", "Battery", "Clock", "Flip Flop", "Button", "Display" };

        public static Component NewComponent(types type)
        {
            switch (type)
            {
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
                    return new WireComp();
            }
        }
        public static Connection NewConnection(types type, Pos pos)
        {
            switch (type)
            {
                case (types.OUT):
                    return new OutConnection(pos);
                case (types.CROSS):
                    return new CrossConnection(pos);
                default:
                    return new InConnection(pos);
            }
        }
    }
}