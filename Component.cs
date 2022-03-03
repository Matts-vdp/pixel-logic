using Raylib_cs;
namespace game {
    enum types {
        WIRE=1,
        BATTERY,
        AND,
        OR,
        NOT,
        OUT,
        IN,
        CLK,
    }

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
    
}