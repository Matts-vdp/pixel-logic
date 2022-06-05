using Raylib_cs;
namespace Game.Components
{
    public class Circuit
    {
        public string Name;
        private readonly Dictionary<int, Component> _components;  // stores all components
        private readonly List<ButtonComp> _buttons;                   // stores buttons for input handling
        private readonly List<Connection> _connections;           // stores all connections between components

        public Circuit(string name, Dictionary<int, Component> components, List<ButtonComp> buttons, List<Connection> connections)
        {
            this.Name = name;
            this._components = components;
            this._buttons = buttons;
            this._connections = connections;
        }

        public Component ToComponent(State state)
        {
            List<Connection> inp = new();
            List<Connection> outp = new();
            Connection? clock = null;

            foreach (Connection i in _connections)
            {
                if (!i.IsFull())
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
        public void Update()
        {
            foreach (Component c in _components.Values)
            {
                if (!(c is WireComp))
                    c.Update();
            }
            foreach (Component c in _components.Values)
            {
                if (c is WireComp)
                    c.Update();
            }
        }
        // handle keyboard input
        public void Input()
        {
            int key = 49;
            for (int i = 0; i < _buttons.Count && i < 9; i++)
            {
                if (Raylib.IsKeyPressed((KeyboardKey)(key + i)))
                {
                    _buttons[i].Toggle();
                }
            }

        }
    }
}