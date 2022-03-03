
namespace game {
        enum types {
        WIRE=1,
        AND,
        OR,
        NOT,
        OUT,
        IN,
        BATTERY,
        CLK,
        FF,
        BUT,
    }
    static class ComponentFactory{
        public static Component NewComponent(types type){
            switch (type) {
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
                default:
                    return new WireComp();
            }
        }
        public static Connection NewConnection(types type, Pos pos){
            switch (type) {
                case (types.OUT):
                    return new OutConnection(pos);
                default:
                    return new InConnection(pos);
            }
        }

        
    }
}