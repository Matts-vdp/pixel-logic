using Raylib_cs;
namespace Game.Components
{
    public class Circuit 
    {
        public string name;
        private Dictionary<int, Component> components;  // stores all components
        private List<ButtonComp> buttons;                   // stores buttons for input handling
        private List<Connection> connections;           // stores all connections between components

        public Circuit(string name, Dictionary<int, Component> components, List<ButtonComp> buttons, List<Connection> connections) 
        {
            this.name = name;
            this.components = components;
            this.buttons = buttons;
            this.connections = connections;
        }

        public Component toComponent(State state)
        {
            List<Connection> inp = new List<Connection>();
            List<Connection> outp = new List<Connection>();
            Connection? clock = null;

            foreach (Connection i in connections)
            {
                if (!i.isFull())
                {
                    if (i.GetType() == typeof(InConnection))
                    {
                        outp.Add(i);
                    }
                    else if (i.GetType() == typeof(OutConnection))
                    {
                        inp.Add(i);
                    }
                    else if (i.GetType() == typeof(ClockIn))
                    {
                        clock = i;
                    }
                }
            }
            return new SubComponent(this, inp, outp, clock, state);
        }
        // update components
        public void update()
        {
            foreach (Component c in components.Values)
            {
                if (!(c is WireComp))
                    c.update();
            }
            foreach (Component c in components.Values)
            {
                if (c is WireComp)
                    c.update();
            }
        }
        // handle keyboard input
        public void Input()
        {
            int key = 49;
            for (int i = 0; i < buttons.Count && i < 9; i++)
            {
                if (Raylib.IsKeyPressed((KeyboardKey)(key + i)))
                {
                    buttons[i].toggle();
                }
            }

        }
    }
}