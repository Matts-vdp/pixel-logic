using Raylib_cs;
namespace Game.Components
{
    // base component holds all shared logic
    public abstract class Component
    {
        protected List<Pos> blocks;            // locations of blocks in grid
        protected List<Connection> inputs;     // all input connections
        protected List<Connection> outputs;    // output connections
        protected Connection? clockIn;         // clock connection if present
        protected bool active {
            get {
                return state.getState(blocks[0]);
            }
            set {
                foreach (Pos p in blocks) {
                    state.setState(p, value);
                }
            }}

        protected State state;

        protected Component(State state)
        {
            blocks = new List<Pos>();
            inputs = new List<Connection>();
            outputs = new List<Connection>();
            this.state = state;
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
    }

    // component that is used to pass signals between other components
    public class WireComp : Component
    {
        public WireComp(State state) : base(state) { }
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
        public static Component newComponent(State state)
        {
            return new WireComp(state);
        }
    }

    // represents and gate
    public class AndComp : Component
    {
        public AndComp(State state) : base(state) { }
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
        public static Component newComponent(State state)
        {
            return new AndComp(state);
        }
    }

    // represents not gate
    public class NotComp : Component
    {
        public NotComp(State state) : base(state) { }
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
        public static Component newComponent(State state)
        {
            return new NotComp(state);
        }
    }

    // represents or gate
    public class OrComp : Component
    {
        public OrComp(State state) : base(state) { }
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
        public static Component newComponent(State state)
        {
            return new OrComp(state);
        }
    }

    // represents xor gate
    public class XorComp : Component
    {
        public XorComp(State state) : base(state) { }
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
        public static Component newComponent(State state)
        {
            return new XorComp(state);
        }
    }

    // represents battery (always 1)
    public class BatteryComp : Component
    {
        public BatteryComp(State state) : base(state)
        {
            active = true;
        }
        // set all outputs to 1
        public override void update()
        {
            active = true;
            foreach (Connection o in outputs)
            {
                o.setActive(true);
            }
        }
        public static Component newComponent(State state)
        {
            return new BatteryComp(state);
        }
    }

    // represents clock
    public class ClockComp : Component
    {
        double time;
        const float DELAY = 0.5f;
        public ClockComp(State state) : base(state)
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
        public static Component newComponent(State state)
        {
            return new ClockComp(state);
        }
    }

    // represents flip flop
    public class FlipFlopComp : Component
    {
        bool lastState = false;
        public FlipFlopComp(State state) : base(state)
        {
        }
        // only update state on change from false to true of "ClockIn"
        public override void update()
        {
            foreach (Connection o in outputs)
                    o.setActive(active);
            if (inputs.Count == 0) return;
            if (clockIn == null) return;

            if (clockIn.isActive() && !lastState)
            {
                active = inputs[0].isActive();
            }
            lastState = clockIn.isActive();
        }
        public override void addClock(Connection c)
        {
            clockIn = c;
        }
        public static Component newComponent(State state)
        {
            return new FlipFlopComp(state);
        }
    }

    // can be changed by keyboard during simulation
    public class ButtonComp : Component
    {
        public ButtonComp(State state) : base(state)
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
        public static Component newComponent(State state)
        {
            return new ButtonComp(state);
        }
    }

    // display input as number
    public class Seg7Comp : Component
    {
        private int value;
        public Seg7Comp(State state) : base(state)
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
        public static Component newComponent(State state)
        {
            return new Seg7Comp(state);
        }
    }

}