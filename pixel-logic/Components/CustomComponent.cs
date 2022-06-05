using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using Raylib_cs;

namespace Game.Components
{
    // contains shared logic for all custom components based on code
    public abstract class CodeComponent : Component
    {
        protected Input _input = new();
        protected CCode _ccode;

        protected CodeComponent(CCode ccode, State state) : base(state)
        {
            this._ccode = ccode;
        }
        // returns inputs as list of states
        private List<bool> GetInputs()
        {
            List<bool> inp = new();
            foreach (Connection c in _inputs)
            {
                inp.Add(c.IsActive());
            }
            return inp;
        }
        // sets the outputs with list of states
        private void SetOutput(List<bool> states)
        {
            for (int i = 0; i < _outputs.Count; i++)
            {
                bool status = false;
                if (i < states.Count)
                    status = states[i];
                _outputs[i].SetActive(status);
            }
        }

        protected void Run()
        {
            _input.I = GetInputs();
            List<bool> output = _ccode.Run(_input);
            SetOutput(output);
        }
    }
    // component with custom code that only gets updated on positive clk 
    public class ProgComp : CodeComponent
    {
        private bool _lastState = false;
        public ProgComp(CCode ccode, State state) : base(ccode, state)
        {
        }

        public override void AddClock(Connection c)
        {
            _clockIn = c;
        }

        public override void Update()
        {
            if (_clockIn == null) return;
            if (_clockIn.IsActive() && !_lastState)
            {
                Run();
            }
            _lastState = _clockIn.IsActive();
        }

    }

    // component with custom code that works 'instant'
    public class CondComp : CodeComponent
    {
        public CondComp(CCode ccode, State state) : base(ccode, state)
        {
        }
        // only calls custom script to update outputs when inputs have changed 
        public override void Update()
        {
            bool change = false;
            foreach (Connection c in _inputs)
            {
                if (c.IsChanged())
                {
                    change = true;
                    break;
                }
            }
            if (!change) return;
            Run();
        }
    }

    // component that contains a grid of components
    public class SubComponent : Component
    {
        private readonly List<Connection> _subInputs; // contains inputs of sub grid
        private readonly List<Connection> _subOutputs;// contains outputs of sub grid
        private readonly Connection? _clock;          // contains ClockIn of sub grid
        private readonly Circuit _circuit;                  // reference to grid with components

        public SubComponent(Circuit c, List<Connection> inp, List<Connection> outp, Connection? clk, State state) : base(state)
        {
            _circuit = c;
            _subInputs = inp;
            _subOutputs = outp;
            _clock = clk;
        }
        public override void AddClock(Connection c)
        {
            _clockIn = c;
        }
        // map inputs to sub inputs then update and read sub outputs to outputs
        public override void Update()
        {
            for (int i = 0; i < _inputs.Count && i < _subInputs.Count; i++)
                _subInputs[i].SetActive(_inputs[i].IsActive());
            if (_clockIn != null)
                _clock?.SetActive(_clockIn.IsActive());
            _circuit.Update();
            for (int i = 0; i < _outputs.Count && i < _subOutputs.Count; i++)
                _outputs[i].SetActive(_subOutputs[i].IsActive());
        }
    }
}