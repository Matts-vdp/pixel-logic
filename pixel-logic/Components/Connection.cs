using Raylib_cs;
namespace Game.Components
{
    // base class containing shared logic for all connections
    public abstract class Connection
    {
        protected Component? _output;
        protected Component? _input;
        protected Pos _pos;             // location in grid
        protected bool Active
        {
            get
            {
                return _state.GetState(_pos);
            }
            set
            {
                _state.SetState(_pos, value);
            }
        }

        private bool _changed;       // state changed since last read

        protected State _state;
        protected Connection(Pos p, State state)
        {
            this._state = state;
            _pos = p;
            Active = false;
            _changed = true;
            if (!IsSub())
                _state.DrawText[_pos] = "";
        }
        // connect input component
        protected void AddInput(Component inp)
        {
            inp.AddOutput(this);
            _input = inp;
        }
        // connect output component
        protected void AddOutput(Component outp)
        {
            outp.AddInput(this);
            _output = outp;
        }

        protected virtual bool IsSub()
        {
            return false;
        }

        public bool IsActive()
        {
            _changed = false;
            return Active;
        }
        public void SetActive(bool value)
        {
            _changed = value != Active;
            Active = value;
        }
        public bool IsChanged()
        {
            return _changed;
        }

        // checks if both sides of the connection are filled
        public bool IsFull()
        {
            return (_input != null) && (_output != null);
        }

        public abstract void AddWire(Component c);
        public abstract void AddOther(Component c);
    }

    // used to pass a signal from a component to a wire
    public class OutConnection : Connection
    {
        public OutConnection(Pos p, State state) : base(p, state)
        {
        }
        public override void AddWire(Component c)
        {
            AddOutput(c);
            if (IsSub())
                _state.DrawText[_pos] = "I";
            else 
                _state.DrawText[_pos] = "";
        }
        public override void AddOther(Component c)
        {
            AddInput(c);
            if (IsSub())
                _state.DrawText[_pos] = "I";
            else 
                _state.DrawText[_pos] = "";
        }
        protected override bool IsSub()
        {
            return (_input == null) && (_output != null);
        }
        public static Connection NewConnection(Pos p, State state)
        {
            return new OutConnection(p, state);
        }
    }

    // used to pass a signal from a wire to a component
    public class InConnection : Connection
    {
        public InConnection(Pos p, State state) : base(p, state)
        {
        }

        public override void AddWire(Component c)
        {
            AddInput(c);
            if (IsSub())
                _state.DrawText[_pos] = "O";
            else 
                _state.DrawText[_pos] = "";
        }
        public override void AddOther(Component c)
        {
            AddOutput(c);
            if (IsSub())
                _state.DrawText[_pos] = "O";
            else 
                _state.DrawText[_pos] = "";
        }
        protected override bool IsSub()
        {
            return (_input == null) && (_output != null);
        }
        
        public static Connection NewConnection(Pos p, State state)
        {
            return new InConnection(p, state);
        }
    }

    // used to pass a signal from a wire to a component using ClockIn for extra functionality
    public class ClockIn : Connection
    {
        public ClockIn(Pos p, State state) : base(p, state)
        {
        }

        public override void AddWire(Component c)
        {
            AddInput(c);
            if (IsSub())
                _state.DrawText[_pos] = "C";
            else 
                _state.DrawText[_pos] = "";
        }
        public override void AddOther(Component c)
        {
            _output = c;
            c.AddClock(this);
            if (IsSub())
                _state.DrawText[_pos] = "C";
            else 
                _state.DrawText[_pos] = "";
        }
        protected override bool IsSub()
        {
            return (_input != null) && (_output == null);
        }
        public static Connection NewConnection(Pos p, State state)
        {
            return new ClockIn(p, state);
        }
    }

    // used to let wires cross each other
    public class CrossConnection : Connection
    {
        public CrossConnection(Pos p, State state) : base(p, state)
        {
        }

        public override void AddWire(Component c)
        {
        }
        public override void AddOther(Component c)
        {
        }
        public static Connection NewConnection(Pos p, State state)
        {
            return new CrossConnection(p, state);
        }
    }
}