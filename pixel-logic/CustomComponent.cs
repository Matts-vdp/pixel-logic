using Raylib_cs;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace game
{
    // contains shared logic for all custom components based on code
    public abstract class CodeComponent : Component
    {
        protected int id;

        protected Input input = new Input();

        public CodeComponent(int i, ComponentList list) : base(list)
        {
            id = i;
        }
        // returns inputs as list of states
        public List<bool> getInputs()
        {
            List<bool> inp = new List<bool>();
            foreach (Connection c in inputs)
            {
                inp.Add(c.isActive());
            }
            return inp;
        }
        // sets the outputs with list of states
        public void setOutput(List<bool> list)
        {
            for (int i = 0; i < outputs.Count; i++)
            {
                bool status = false;
                if (i < list.Count)
                    status = list[i];
                outputs[i].setActive(status);
            }
        }

        public void run() {
            input.i = getInputs();
            List<bool> output = ((CCode)list.components[id]).run(input);
            setOutput(output);
        }

        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = new Color(100, 100, 100, 255);
            string s = list.components[id].name[0].ToString();
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
                Raylib.DrawText(s, pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, Color.WHITE);
            }
        }
    }
    // component with custom code that only gets updated on positive clk 
    public class ProgComp : CodeComponent
    {
        bool lastState = false;
        public ProgComp(int i, ComponentList list) : base(i, list)
        {
        }

        public override void addClock(Connection c)
        {
            clockIn = c;
        }

        public override void update()
        {
            if (clockIn == null) return;
            if (!list.components.ContainsKey(id))
                return;
            if (clockIn.isActive() && !lastState)
            {
                run();
            }
            lastState = clockIn.isActive();
        }

    }

    // component with custom code that works 'instant'
    public class CondComp : CodeComponent
    {
        public CondComp(int i, ComponentList list) : base(i, list)
        {
        }
        // only calls custom script to update outputs when inputs have changed 
        public override void update()
        {
            bool change = false;
            foreach (Connection c in inputs)
            {
                if (c.isChanged())
                {
                    change = true;
                    break;
                }
            }
            if (!change) return;
            if (!list.components.ContainsKey(id))
                return;
            run();
        }
    }

    // component that contains a grid of components
    public class SubComponent : Component
    {
        private List<Connection> subInputs; // contains inputs of sub grid
        private List<Connection> subOutputs;// contains outputs of sub grid
        private Connection? clock;          // contains ClockIn of sub grid
        private Grid grid;                  // reference to grid with components

        public SubComponent(Grid g, List<Connection> inp, List<Connection> outp, Connection? clk) : base(g.list)
        {
            grid = g;
            subInputs = inp;
            subOutputs = outp;
            clock = clk;
        }
        public override void addClock(Connection c)
        {
            clockIn = c;
        }
        // map inputs to sub inputs then update and read sub outputs to outputs
        public override void update()
        {
            for (int i = 0; i < inputs.Count && i < subInputs.Count; i++)
                subInputs[i].setActive(inputs[i].isActive());
            if (clockIn != null)
                clock?.setActive(clockIn.isActive());
            grid.update();
            for (int i = 0; i < outputs.Count && i < subOutputs.Count; i++)
                outputs[i].setActive(subOutputs[i].isActive());
        }
        public override void draw(int gridsize, int xoff, int yoff)
        {
            Color color = new Color(78, 255, 50, 255);
            foreach (Pos pos in blocks)
            {
                Raylib.DrawRectangle(pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, gridsize, color);
                Raylib.DrawText(grid.name[0].ToString(), pos.x * gridsize - xoff, pos.y * gridsize - yoff, gridsize, Color.WHITE);
            }
        }
    }
}