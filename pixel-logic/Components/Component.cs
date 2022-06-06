using Raylib_cs;
namespace Game.Components
{
    // base component holds all shared logic
    public abstract class Component
    {
        protected List<Pos> _blocks;            // locations of blocks in grid
        protected List<Connection> _inputs;     // all input connections
        protected List<Connection> _outputs;    // output connections
        protected Connection? _clockIn;         // clock connection if present
        protected bool Active
        {
            get
            {
                return _state.GetState(_blocks[0]);
            }
            set
            {
                foreach (Pos p in _blocks)
                {
                    _state.SetState(p, value);
                }
            }
        }

        protected State _state;

        protected Component(State state)
        {
            _blocks = new List<Pos>();
            _inputs = new List<Connection>();
            _outputs = new List<Connection>();
            this._state = state;
        }
        // add new block to the component
        public void Add(Pos p)
        {
            _blocks.Add(p);
        }
        // add a input to the component
        public void AddInput(Connection c)
        {
            _inputs.Add(c);
        }
        // add a input to the component
        public void AddOutput(Connection c)
        {
            _outputs.Add(c);
        }
        // add clock to inputs subClasses can override this to use clock
        public virtual void AddClock(Connection c)
        {
            _inputs.Add(c);
        }

        public abstract void Update();
    }

    // component that is used to pass signals between other components
    public class WireComp : Component
    {
        public WireComp(State state) : base(state) { }
        // pass input to output, true if inputs true and false
        public override void Update()
        {
            bool value = false;
            foreach (Connection i in _inputs)
            {
                if (i.IsActive())
                {
                    value = true;
                    break;
                }
                value = false;
            }
            Active = value;
            foreach (Connection o in _outputs)
            {
                o.SetActive(value);
            }
        }
        public static Component NewComponent(State state)
        {
            return new WireComp(state);
        }
    }

    // represents and gate
    public class AndComp : Component
    {
        public AndComp(State state) : base(state) { }
        // output = input0 && input1 && ...
        public override void Update()
        {
            bool value = false;
            foreach (Connection i in _inputs)
            {
                if (!i.IsActive())
                {
                    value = false;
                    break;
                }
                value = true;
            }
            Active = value;
            foreach (Connection o in _outputs)
            {
                o.SetActive(value);
            }
        }
        public static Component NewComponent(State state)
        {
            return new AndComp(state);
        }
    }

    // represents not gate
    public class NotComp : Component
    {
        public NotComp(State state) : base(state) { }
        // output = ! input
        public override void Update()
        {
            bool value = false;
            foreach (Connection i in _inputs)
            {
                if (!i.IsActive())
                {
                    value = true;
                    break;
                }
                value = false;
            }
            Active = value;
            foreach (Connection o in _outputs)
            {
                o.SetActive(value);
            }
        }
        public static Component NewComponent(State state)
        {
            return new NotComp(state);
        }
    }

    // represents or gate
    public class OrComp : Component
    {
        public OrComp(State state) : base(state) { }
        // output = input0 || input1 ...
        public override void Update()
        {
            bool value = false;
            foreach (Connection i in _inputs)
            {
                if (i.IsActive())
                {
                    value = true;
                    break;
                }
                value = false;
            }
            Active = value;
            foreach (Connection o in _outputs)
            {
                o.SetActive(value);
            }
        }
        public static Component NewComponent(State state)
        {
            return new OrComp(state);
        }
    }

    // represents xor gate
    public class XorComp : Component
    {
        public XorComp(State state) : base(state) { }
        public override void Update()
        {
            bool i1 = false;
            bool i2 = false;
            if (_inputs.Count > 0) { i1 = _inputs[0].IsActive(); }
            if (_inputs.Count > 1) { i2 = _inputs[1].IsActive(); }
            Active = i1 ^ i2;
            foreach (Connection o in _outputs)
            {
                o.SetActive(Active);
            }
        }
        public static Component NewComponent(State state)
        {
            return new XorComp(state);
        }
    }

    // represents battery (always 1)
    public class BatteryComp : Component
    {
        public BatteryComp(State state) : base(state)
        {
            Active = true;
        }
        // set all outputs to 1
        public override void Update()
        {
            Active = true;
            foreach (Connection o in _outputs)
            {
                o.SetActive(true);
            }
        }
        public static Component NewComponent(State state)
        {
            return new BatteryComp(state);
        }
    }

    // represents clock
    public class ClockComp : Component
    {
        double _time;
        const float DELAY = 0.5f;
        public ClockComp(State state) : base(state)
        {
            Active = false;
            _time = Raylib.GetTime();
        }
        // switches every "DELAY' seconds between true and false
        public override void Update()
        {
            double newTime = Raylib.GetTime();
            if ((newTime - _time) > DELAY)
            {
                _time = newTime;
                Active = !Active;
                foreach (Connection o in _outputs)
                {
                    o.SetActive(Active);
                }
            }
        }
        public static Component NewComponent(State state)
        {
            return new ClockComp(state);
        }
    }

    // represents flip flop
    public class FlipFlopComp : Component
    {
        bool _lastState = false;
        public FlipFlopComp(State state) : base(state)
        {
        }
        // only update state on change from false to true of "ClockIn"
        public override void Update()
        {
            foreach (Connection o in _outputs)
                o.SetActive(Active);
            if (_inputs.Count == 0) return;
            if (_clockIn == null) return;

            if (_clockIn.IsActive() && !_lastState)
            {
                Active = _inputs[0].IsActive();
            }
            _lastState = _clockIn.IsActive();
        }
        public override void AddClock(Connection c)
        {
            _clockIn = c;
        }
        public static Component NewComponent(State state)
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
        public override void Update()
        {
        }

        // used by grid to toggle button on key press
        public void Toggle()
        {
            Active = !Active;
            foreach (Connection o in _outputs)
            {
                o.SetActive(Active);
            }
        }
        public static Component NewComponent(State state)
        {
            return new ButtonComp(state);
        }
    }

    // display input as number
    public class Seg7Comp : Component
    {
        public Seg7Comp(State state) : base(state)
        {
        }
        // read input and save as int
        public override void Update()
        {
            int num = 0;
            for (int i = 0; i < _inputs.Count; i++)
            {
                if (_inputs[i].IsActive())
                {
                    num += (int)Math.Pow(2, i);
                }
            }
            _state.SetText(_blocks, num.ToString());
        }
        public static Component NewComponent(State state)
        {
            return new Seg7Comp(state);
        }
    }

}