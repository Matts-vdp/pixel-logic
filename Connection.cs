using Raylib_cs;
namespace game {
    abstract class Connection {
        Component? output;
        Component? input;

        public Pos pos;
        public bool active;

        public Connection(Pos p) {
            pos = p;
            active = false;
        }

        public void addInput(Component inp) {
            inp.addOutput(this);
            input = inp;
        }
        public void addOutput(Component outp) {
            outp.addInput(this);
            output = outp;
        }

        public void update(){
            input?.update();
            output?.update();
        }

        public bool isActive(){
            return active;
        }
        public void setActive(bool value){
            active = value;
            // output?.update();
        }
        public abstract void draw(int gridsize, int xoff, int yoff);
        public abstract void addWire(Component c);
        public abstract void addOther(Component c);
    }

    class OutConnection : Connection {
        public OutConnection(Pos p) : base(p){
        }
        public override void draw(int gridsize, int xoff, int yoff){
            Color color = active? Color.BLUE : Color.DARKBLUE;
            Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
        }
        public override void addWire(Component c){
            addOutput(c);
        }
        public override void addOther(Component c){
            addInput(c);
        }
    }

    class InConnection : Connection {
        public InConnection(Pos p) : base(p){
        }

        public override void draw(int gridsize, int xoff, int yoff){
            Color color = active? Color.RED : Color.MAROON;
            Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
        }

        public override void addWire(Component c){
            addInput(c);
        }
        public override void addOther(Component c){
            addOutput(c);
        }
    }
}