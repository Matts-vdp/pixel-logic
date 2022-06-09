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
        protected Value Active
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
            _state = state;
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
            // combine all inputs
            Value value = new();
            foreach (Connection i in _inputs)
            {
                Value inp = i.IsActive();
                if (inp.Count ==0)
                    inp[0] = false;
                value.Add(i.IsActive());
            }
            // set active
            Active = value;
            // push to output
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
            Value value = new();
            if (_inputs.Count > 0)
            {
                value.Add(_inputs[0].IsActive());
                foreach (Connection inp in _inputs)
                {
                    Value other = inp.IsActive();
                    int mx = Math.Max(value.Count, other.Count);
                    for (int i = 0; i < mx; i++)
                    {
                        value[i] = value[i] & other[i];
                    }
                }
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
            Value value = new();

            // or al inputs
            foreach (Connection inp in _inputs)
            {
                Value other = inp.IsActive();
                for (int i = 0; i < other.Count; i++)
                {
                    value[i] = value[i] | other[i];
                }
            }
            // invert input
            for (int i = 0; i < value.Count; i++)
                value[i] = !value[i];

            // push to output
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
            Value value = new();

            // or al inputs
            foreach (Connection inp in _inputs)
            {
                Value other = inp.IsActive();
                for (int i = 0; i < other.Count; i++)
                {
                    value[i] = value[i] | other[i];
                }
            }

            // push to output
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
            Value value = new();
            if (_inputs.Count > 0)
            {
                // value.Add(_inputs[0].IsActive());
                foreach (Connection inp in _inputs)
                {
                    Value other = inp.IsActive();
                    int mx = Math.Max(value.Count, other.Count);
                    for (int i = 0; i < mx; i++)
                    {
                        value[i] = value[i] ^ other[i];
                    }
                }
            }

            // push to output
            Active = value;
            foreach (Connection o in _outputs)
            {
                o.SetActive(value);
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
            Active = Value.True();
        }
        // set all outputs to 1
        public override void Update()
        {
            Value value = Value.True();
            Active = value;
            foreach (Connection o in _outputs)
            {
                o.SetActive(value);
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
        public ClockComp(State state) : base(state)
        {
            Active.Reset();
        }

        public void SetState(bool state)
        {
            Value value = Active;
            value[0] = state;
        }

        // switches every "DELAY' seconds between true and false
        public override void Update()
        {
            foreach (Connection o in _outputs)
            {
                o.SetActive(Active);
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

            if (_clockIn.IsActive()[0] && !_lastState)
            {
                Active = _inputs[0].IsActive();
            }
            _lastState = _clockIn.IsActive()[0];
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
            foreach (Connection o in _outputs)
            {
                o.SetActive(Active);
            }
        }

        // used by grid to toggle button on key press
        public void Toggle()
        {
            Value value = Active;
            value[0] = !value[0];
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
            // combine all inputs
            Value value = new();
            foreach (Connection i in _inputs)
            {
                value.Add(i.IsActive());
            }
            // convert to int
            int num = 0;
            for (int i = 0; i < value.Count; i++)
            {
                if (value[i])
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

    public class SplitComp : Component
    {
        public SplitComp(State state) : base(state) { }
        public override void Update()
        {
            // combine all inputs
            Value value = new();
            foreach (Connection i in _inputs)
            {
                value.Add(i.IsActive());
            }

            // push to output
            for (int i=0; i<_outputs.Count-1; i++)
            {
                Value val = new();
                val[0] = value[i];
                _outputs[i].SetActive(val);
            }
            int last = _outputs.Count-1;
            if (last > 0)
            {
                Value lastVal = new();
                if (value.Count > last)
                {
                    // add others to the last output
                    value.Values.RemoveRange(0, last);
                    lastVal.Add(value);
                }
                _outputs[last].SetActive(lastVal);
            }
        }
        public static Component NewComponent(State state)
        {
            return new SplitComp(state);
        }
    }

}