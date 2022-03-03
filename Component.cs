using Raylib_cs;
namespace game {
    abstract class Component {
        public List<Pos> blocks;
        public List<Connection> inputs;
        public List<Connection> outputs;
        public bool active = false;

        public Component() {
            blocks = new List<Pos>();
            inputs = new List<Connection>();
            outputs = new List<Connection>();
        }
        
        public void add(Pos p) {
            blocks.Add(p);
        }
        public void addInput(Connection c) {
            inputs.Add(c);
        }
        public void addOutput(Connection c) {
            outputs.Add(c);
        }
        public abstract void update();
        public abstract void draw(int gridsize, int xoff, int yoff);
    }

    class WireComp : Component {
        public WireComp() : base() {}
        public override void update() {
            bool value = false;
            foreach (Connection i in inputs) {
                if (i.isActive()) {
                    value = true;
                    break;
                }
                value = false;
            }
            active = value;
            foreach (Connection o in outputs) {
                o.setActive(value);
            }
        }

        public override void draw(int gridsize, int xoff, int yoff){
            Color color = active? Color.YELLOW : Color.BROWN;
            foreach (Pos pos in blocks) {
                Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
            }
        }
    }

    class AndComp : Component {
        public AndComp() : base() {}
        public override void update() {
            bool value = false;
            foreach (Connection i in inputs) {
                if (!i.isActive()) {
                    value = false;
                    break;
                }
                value = true;
            }
            active = value;
            foreach (Connection o in outputs) {
                o.setActive(value);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff){
            Color color = active? Color.GREEN : Color.DARKGREEN;
            foreach (Pos pos in blocks) {
                Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
            }
        }
    }

        class NotComp : Component {
        public NotComp() : base() {}
        public override void update() {
            bool value = false;
            foreach (Connection i in inputs) {
                if (!i.isActive()) {
                    value = true;
                    break;
                }
                value = false;
            }
            active = value;
            foreach (Connection o in outputs) {
                o.setActive(value);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff){
            Color color = active? Color.ORANGE : Color.ORANGE;
            foreach (Pos pos in blocks) {
                Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
            }
        }
    }

    class OrComp : Component {
        public OrComp() : base() {}
        public override void update() {
            bool value = false;
            foreach (Connection i in inputs) {
                if (i.isActive()) {
                    value = true;
                    break;
                }
                value = false;
            }
            active = value;
            foreach (Connection o in outputs) {
                o.setActive(value);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff){
            Color color = active? Color.PURPLE : Color.PURPLE;
            foreach (Pos pos in blocks) {
                Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
            }
        }
    }

    class XorComp : Component {
        public XorComp() : base() {}
        public override void update() {
            bool i1 = false;
            bool i2 = false;
            if (inputs.Count > 0){i1 = inputs[0].isActive();}
            if (inputs.Count > 1){i2 = inputs[1].isActive();}
            active = i1 ^ i2;
            foreach (Connection o in outputs) {
                o.setActive(active);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff){
            Color color = active? new Color(125,255,0,255) : new Color(125,125,0,255);
            foreach (Pos pos in blocks) {
                Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
            }
        }
    }

    class BatComp : Component {
        public BatComp() : base() {
            active = true;
        }
        public override void update() {
            foreach (Connection o in outputs) {
                o.setActive(true);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff){
            Color color = Color.GOLD;
            foreach (Pos pos in blocks) {
                Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
            }
        }
    }

    class Clock : Component {
        double time;
        public Clock() : base() {
            active = false;
            time = Raylib.GetTime();
        }
        public override void update() {
            double newTime = Raylib.GetTime();
            if ((newTime - time) > 0.5) {
                time = newTime;
                active = !active;
                foreach (Connection o in outputs) {
                    o.setActive(active);
                }
            }
        }
        public override void draw(int gridsize, int xoff, int yoff){
            Color color = Color.MAGENTA;
            foreach (Pos pos in blocks) {
                Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
            }
        }
    }

    class FlipFlop : Component {

        bool lastState = false;
        public FlipFlop() : base() {
        }
        public override void update() {
            if (inputs.Count==0) {return;}
            if (inputs.Last()!.isActive() && !lastState){
                active = inputs[0].isActive();
            }
            foreach (Connection o in outputs) {
                o.setActive(active);
            }
            lastState = inputs.Last()!.isActive();
        }
        public override void draw(int gridsize, int xoff, int yoff){
            Color color = active? new Color(0,255,255,255): new Color(0,100,100,255);
            foreach (Pos pos in blocks) {
                Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
            }
        }
    }
    
    class Button : Component {
        public Button() : base() {
        }
        public override void update() {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_B)) {
                active = !active;
                foreach (Connection o in outputs) {
                    o.setActive(active);
                }
            }
        }
        public override void draw(int gridsize, int xoff, int yoff){
            Color color = active? new Color(0,255,255,255): new Color(0,100,100,255);
            foreach (Pos pos in blocks) {
                Raylib.DrawRectangle(pos.x*gridsize-xoff, pos.y*gridsize-yoff, gridsize, gridsize, color);
            }
        }
    }
    
}