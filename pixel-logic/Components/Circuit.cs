using Raylib_cs;
namespace Game.Components
{
    public class Circuit
    {
        public string Name;
        private readonly Dictionary<int, Component> _components;  // stores all components
        private readonly List<ButtonComp> _buttons;                   // stores buttons for input handling
        private readonly List<ClockComp> _clocks;
        private readonly List<Connection> _connections;           // stores all connections between components
        private double _time;

        public Circuit(
            string name, 
            Dictionary<int, Component> components, 
            List<ButtonComp> buttons, 
            List<ClockComp> clocks,
            List<Connection> connections
            )
        {
            Name = name;
            _components = components;
            _buttons = buttons;
            _clocks = clocks;
            _connections = connections;
            _time = Raylib.GetTime();
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

        private void UpdateClock(int clockTime = -1)
        {
            if (clockTime == -1)
                return;
            double now = Raylib.GetTime();
            double diff = (now - _time) * 1000;
            // update clocks
            if (diff > (clockTime/2))
            {
                bool value = true;
                if (diff > clockTime) 
                {
                    value = false;
                    _time = now;
                }
                foreach (ClockComp c in _clocks)
                    c.SetState(value);
            }
        }


        public void Update(CancellationToken? ct = null, int clockTime = -1)
        {
            UpdateClock(clockTime);
            
            // update components
            foreach (Component c in _components.Values)
            {
                if (ct is CancellationToken token)
                    token.ThrowIfCancellationRequested();
                if (c is not WireComp)
                    c.Update();
            }
            // update wires
            foreach (Component c in _components.Values)
            {
                if (ct is CancellationToken token)
                    token.ThrowIfCancellationRequested();
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