using Raylib_cs;
namespace game
{
    abstract class Component
    {
        public List<Pos> blocks;
        public List<Connection> inputs;
        public List<Connection> outputs;
        public Connection? clockIn;
        public bool active = false;
        protected ComponentList list;

        public Component(ComponentList list)
        {
            blocks = new List<Pos>();
            inputs = new List<Connection>();
            outputs = new List<Connection>();
            this.list = list;
        }
        public void add(Pos p)
        {
            blocks.Add(p);
        }
        public void addInput(Connection c)
        {
            inputs.Add(c);
        }
        public void addOutput(Connection c)
        {
            outputs.Add(c);
        }
        // add clock to inputs subClasses can override this to use clock
        public virtual void addClock(Connection c) {
            inputs.Add(c);
        }

        public abstract void update();
        public abstract void draw(int gridsize, int xoff, int yoff);
    }

    class WireComp : Component
    {
        public WireComp(ComponentList list) : base(list) { }
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

    class AndComp : Component
    {
        public AndComp(ComponentList list) : base(list) { }
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

    class NotComp : Component
    {
        public NotComp(ComponentList list) : base(list) { }
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

    class OrComp : Component
    {
        public OrComp(ComponentList list) : base(list) { }
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

    class BatComp : Component
    {
        public BatComp(ComponentList list) : base(list)
        {
            active = true;
        }
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

    class Clock : Component
    {
        double time;
        public Clock(ComponentList list) : base(list)
        {
            active = false;
            time = Raylib.GetTime();
        }
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

    class FlipFlop : Component
    {
        bool lastState = false;
        public FlipFlop(ComponentList list) : base(list)
        {
        }
        public override void update()
        {
            if (inputs.Count == 0) return;
            if (clockIn == null) return;

            if (clockIn.isActive() && !lastState) {
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
        public override void addClock(Connection c){
            clockIn = c;
        }
    }

    class Button : Component
    {
        public Button(ComponentList list) : base(list)
        {
        }
        public override void update()
        {
        }

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

    class Seg7 : Component
    {
        private int value;
        public Seg7(ComponentList list) : base(list)
        {
        }
        public override void update()
        {
            int num = 0;
            for (int i=0; i<inputs.Count; i++) 
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
                Raylib.DrawText(value.ToString(),pos.x * gridsize - xoff , pos.y * gridsize - yoff, gridsize, Color.WHITE);
            }
        }
    }

    // component with custom code that only gets updated on positive clk 
    class ProgComp : Component
    {
        public int id;
        bool lastState = false;
        public ProgComp(int i, ComponentList list) : base(list)
        {
            id = i;
        }

        public override void addClock(Connection c)
        {
            clockIn = c;
        }
        public List<bool> getInputs(){
            List<bool> inp = new List<bool>();
            foreach (Connection c in inputs) {
                inp.Add(c.isActive());
            }
            return inp;
        }
        public void setOutput(List<bool> list) {
            for (int i=0; i<outputs.Count; i++) {
                bool status = false;
                if (i < list.Count)
                    status = list[i];
                outputs[i].setActive(status);
            }
        }
        public override void update()
        {  
            if (clockIn == null) return;
            if (!list.components.ContainsKey(id))
                return;
            if (clockIn.isActive() && !lastState) {
                List<bool> output = ((CCode)list.components[id]).run(getInputs());
                setOutput(output);
            }
            lastState = clockIn.isActive();
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = new Color(100, 100, 100, 255);
            var a = list.components;
            string s = list.components[id].name[0].ToString();
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
                Raylib.DrawText(s ,pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, Color.WHITE);
            }
        }
    }
    
    // component with custom code that works 'instant'
    class CondComp : Component
    {
        public int id;
        public CondComp(int i, ComponentList list) : base(list)
        {
            id = i;
        }
        public List<bool> getInputs(){
            List<bool> inp = new List<bool>();
            foreach (Connection c in inputs) {
                inp.Add(c.isActive());
            }
            return inp;
        }
        public void setOutput(List<bool> list) {
            for (int i=0; i<outputs.Count; i++) {
                bool status = false;
                if (i < list.Count)
                    status = list[i];
                outputs[i].setActive(status);
            }
        }
        public override void update()
        {  
            bool change = false;
            foreach (Connection c in inputs) {
                if (c.isChanged()) {
                    change = true;
                    break;
                }
            }
            if (!change) return;
            if (!list.components.ContainsKey(id))
                return;
            List<bool> output = ((CCode)list.components[id]).run(getInputs());
            setOutput(output);
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = new Color(100, 100, 100, 255);
            var a = list.components;
            string s = list.components[id].name[0].ToString();
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
                Raylib.DrawText(s ,pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, Color.WHITE);
            }
        }
    }

    // component that contains a circuit of components
    class SubComponent : Component
    {
        private List<Connection> subInp;

        private List<Connection> subOutp;
        private Connection? clock;

        private Grid grid;

        public SubComponent(
            Grid g, 
            List<Connection> inp, 
            List<Connection> outp, 
            Connection? clk
            ) : base(g.list)
        {   
            grid = g;
            subInp = inp;
            subOutp = outp;
            clock = clk;
        }
        public override void addClock(Connection c)
        {
            clockIn = c;
        }
        public override void update()
        {
            for (int i=0; i<inputs.Count && i<subInp.Count; i++) 
                subInp[i].setActive(inputs[i].isActive());
            if (clockIn != null)
                clock?.setActive(clockIn.isActive());
            grid.update();
            for (int i=0; i<outputs.Count && i<subOutp.Count; i++) 
                outputs[i].setActive(subOutp[i].isActive());
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = new Color(78, 255, 50, 255);
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
                Raylib.DrawText(grid.name[0].ToString(), pos.x * gridsize - xoff , pos.y * gridsize - yoff, gridsize, Color.WHITE);
            }
        }
    }
}