using Raylib_cs;
namespace game
{
    // base class containing shared logic for all connections
    public abstract class Connection
    {
        protected Component? output;
        protected Component? input;
        protected Pos pos;             // location in grid
        protected bool active;         // state

        private bool changed;       // state changed since last read

        protected Connection(Pos p)
        {
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
        private void updateIn()
        {
            input?.update();
        }
        private void updateOut()
        {
            output?.update();
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

        public abstract void draw(int gridsize, int xoff, int yoff);
        public abstract void addWire(Component c);
        public abstract void addOther(Component c);
    }

    // used to pass a signal from a component to a wire
    public class OutConnection : Connection
    {
        public OutConnection(Pos p) : base(p)
        {
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? Color.BLUE : Color.DARKBLUE;
            Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            if (input == null && output != null)
                Raylib.DrawText(" I", pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, Color.WHITE);
        }
        public override void addWire(Component c)
        {
            addOutput(c);
        }
        public override void addOther(Component c)
        {
            addInput(c);
        }
    }

    // used to pass a signal from a wire to a component
    public class InConnection : Connection
    {
        public InConnection(Pos p) : base(p)
        {
        }

        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? Color.RED : Color.MAROON;
            Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            if (output == null && input != null)
                Raylib.DrawText("O", pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, Color.WHITE);
        }

        public override void addWire(Component c)
        {
            addInput(c);
        }
        public override void addOther(Component c)
        {
            addOutput(c);
        }
    }

    // used to pass a signal from a wire to a component using ClockIn for extra functionality
    public class ClockIn : Connection
    {
        public ClockIn(Pos p) : base(p)
        {
        }

        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? new Color(255, 50, 100, 255) : Color.RED;
            Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
            if (input == null && output != null)
                Raylib.DrawText("C", pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, Color.WHITE);
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
    }

    // used to let wires cross each other
    public class CrossConnection : Connection
    {
        public CrossConnection(Pos p) : base(p)
        {
        }

        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = active ? Color.GRAY : Color.GRAY;
            Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
        }

        public override void addWire(Component c)
        {
        }
        public override void addOther(Component c)
        {
        }
    }
}