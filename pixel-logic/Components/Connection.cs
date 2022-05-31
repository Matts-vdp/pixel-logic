using Raylib_cs;
namespace Game.Components
{
    // base class containing shared logic for all connections
    public abstract class Connection
    {
        protected Component? output;
        protected Component? input;
        protected Pos pos;             // location in grid
        protected bool active {
            get {
                return state.getState(pos);
            }
            set {
                state.setState(pos, value);
            }}

        private bool changed;       // state changed since last read

        protected State state;
        protected Connection(Pos p, State state)
        {
            this.state = state;
            pos = p;
            active = false;
            changed = true;
        }
        // connect input component
        protected void addInput(Component inp)
        {
            inp.addOutput(this);
            input = inp;
        }
        // connect output component
        protected void addOutput(Component outp)
        {
            outp.addInput(this);
            output = outp;
        }
        public bool isActive()
        {
            changed = false;
            return active;
        }
        public void setActive(bool value)
        {
            changed = value != active;
            active = value;
        }
        public bool isChanged()
        {
            return changed;
        }

        // checks if both sides of the connection are filled
        public bool isFull()
        {
            return (input != null) && (output != null);
        }

        public abstract void addWire(Component c);
        public abstract void addOther(Component c);
    }

    // used to pass a signal from a component to a wire
    public class OutConnection : Connection
    {
        public OutConnection(Pos p, State state) : base(p, state)
        {
        }
        public override void addWire(Component c)
        {
            addOutput(c);
        }
        public override void addOther(Component c)
        {
            addInput(c);
        }
        public static Connection newConnection(Pos p, State state)
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

        public override void addWire(Component c)
        {
            addInput(c);
        }
        public override void addOther(Component c)
        {
            addOutput(c);
        }
        public static Connection newConnection(Pos p, State state)
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

        public override void addWire(Component c)
        {
            addInput(c);
        }
        public override void addOther(Component c)
        {
            output = c;
            c.addClock(this);
        }
        public static Connection newConnection(Pos p, State state)
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

        public override void addWire(Component c)
        {
        }
        public override void addOther(Component c)
        {
        }
        public static Connection newConnection(Pos p, State state)
        {
            return new CrossConnection(p, state);
        }
    }
}