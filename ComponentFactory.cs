
namespace game {

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