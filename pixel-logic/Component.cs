using Raylib_cs;
namespace game
{
    // base component holds all shared logic
    abstract class Component
    {
        public List<Pos> blocks;            // locations of blocks in grid
        public List<Connection> inputs;     // all input connections
        public List<Connection> outputs;    // output connections
        public Connection? clockIn;         // clock connection if present
        public bool active = false;         // state of component
        protected ComponentList list;       // stores the component index 
                                            // used to retreive custom component data

        public Component(ComponentList list)
        {
            blocks = new List<Pos>();
            inputs = new List<Connection>();
            outputs = new List<Connection>();
            this.list = list;
        }
        // add new block to the component
        public void add(Pos p)
        {
            blocks.Add(p);
        }
        // add a input to the component
        public void addInput(Connection c)
        {
            inputs.Add(c);
        }
        // add a input to the component
        public void addOutput(Connection c)
        {
            outputs.Add(c);
        }
        // add clock to inputs subClasses can override this to use clock
        public virtual void addClock(Connection c)
        {
            inputs.Add(c);
        }

        public abstract void update();
        public abstract void draw(int gridsize, int xoff, int yoff);
    }

    // component that is used to pass signals between other components
    class WireComp : Component
    {
        public WireComp(ComponentList list) : base(list) { }
        // pass input to output, true if inputs true and false
        public override void update()
        {
            bool value = false;
            foreach (Connection i in inputs)
            {
                if (i.isActive())
                {
                    value = true;
                    break;
                }
                value = false;
            }
            active = value;
            foreach (Connection o in outputs)
            {
                o.setActive(value);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? Color.YELLOW : Color.BROWN;
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            }
        }
    }

    // represents and gate
    class AndComp : Component
    {
        public AndComp(ComponentList list) : base(list) { }
        // output = input0 && input1 && ...
        public override void update()
        {
            bool value = false;
            foreach (Connection i in inputs)
            {
                if (!i.isActive())
                {
                    value = false;
                    break;
                }
                value = true;
            }
            active = value;
            foreach (Connection o in outputs)
            {
                o.setActive(value);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? Color.GREEN : Color.DARKGREEN;
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            }
        }
    }

    // represents not gate
    class NotComp : Component
    {
        public NotComp(ComponentList list) : base(list) { }
        // output = ! input
        public override void update()
        {
            bool value = false;
            foreach (Connection i in inputs)
            {
                if (!i.isActive())
                {
                    value = true;
                    break;
                }
                value = false;
            }
            active = value;
            foreach (Connection o in outputs)
            {
                o.setActive(value);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? Color.ORANGE : Color.ORANGE;
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            }
        }
    }

    // represents or gate
    class OrComp : Component
    {
        public OrComp(ComponentList list) : base(list) { }
        // output = input0 || input1 ...
        public override void update()
        {
            bool value = false;
            foreach (Connection i in inputs)
            {
                if (i.isActive())
                {
                    value = true;
                    break;
                }
                value = false;
            }
            active = value;
            foreach (Connection o in outputs)
            {
                o.setActive(value);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? Color.PURPLE : Color.PURPLE;
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            }
        }
    }

    // represents xor gate
    class XorComp : Component
    {
        public XorComp(ComponentList list) : base(list) { }
        public override void update()
        {
            bool i1 = false;
            bool i2 = false;
            if (inputs.Count > 0) { i1 = inputs[0].isActive(); }
            if (inputs.Count > 1) { i2 = inputs[1].isActive(); }
            active = i1 ^ i2;
            foreach (Connection o in outputs)
            {
                o.setActive(active);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? new Color(125, 255, 0, 255) : new Color(125, 125, 0, 255);
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            }
        }
    }

    // represents battery (always 1)
    class BatComp : Component
    {
        public BatComp(ComponentList list) : base(list)
        {
            active = true;
        }
        // set all outputs to 1
        public override void update()
        {
            foreach (Connection o in outputs)
            {
                o.setActive(true);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = Color.GOLD;
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            }
        }
    }

    // represents clock
    class Clock : Component
    {
        double time;
        const float DELAY = 0.5f;
        public Clock(ComponentList list) : base(list)
        {
            active = false;
            time = Raylib.GetTime();
        }
        // switches every "DELAY' seconds between true and false
        public override void update()
        {
            double newTime = Raylib.GetTime();
            if ((newTime - time) > 0.5)
            {
                time = newTime;
                active = !active;
                foreach (Connection o in outputs)
                {
                    o.setActive(active);
                }
            }
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = Color.MAGENTA;
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            }
        }
    }

    // represents flip flop
    class FlipFlop : Component
    {
        bool lastState = false;
        public FlipFlop(ComponentList list) : base(list)
        {
        }
        // only update state on change from false to true of "ClockIn"
        public override void update()
        {
            if (inputs.Count == 0) return;
            if (clockIn == null) return;

            if (clockIn.isActive() && !lastState)
            {
                active = inputs[0].isActive();

                foreach (Connection o in outputs)
                    o.setActive(active);
            }
            lastState = clockIn.isActive();
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? new Color(0, 255, 255, 255) : new Color(0, 100, 100, 255);
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            }
        }
        public override void addClock(Connection c)
        {
            clockIn = c;
        }
    }

    // can be changed by keyboard during simulation
    class Button : Component
    {
        public Button(ComponentList list) : base(list)
        {
        }
        public override void update()
        {
        }

        // used by grid to toggle button on key press
        public void toggle()
        {
            active = !active;
            foreach (Connection o in outputs)
            {
                o.setActive(active);
            }
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? new Color(0, 255, 255, 255) : new Color(0, 100, 100, 255);
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            }
        }
    }

    // display input as number
    class Seg7 : Component
    {
        private int value;
        public Seg7(ComponentList list) : base(list)
        {
        }
        // read input and save as int
        public override void update()
        {
            int num = 0;
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].isActive())
                {
                    num += (int)Math.Pow(2, i);
                }
            }
            value = num;
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? new Color(78, 255, 50, 255) : new Color(0, 100, 0, 255);
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
                Raylib.DrawText(value.ToString(), pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, Color.WHITE);
            }
        }
    }
}