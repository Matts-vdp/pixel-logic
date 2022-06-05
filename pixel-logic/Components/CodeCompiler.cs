using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using Raylib_cs;

namespace Game.Components
{
    // used by the custom script to interact with the logic
    // the attributes and functions of this class can be used in the script
    public class Input
    {
        public List<bool> I;    //input connections
        public List<bool> O;    // output connections
        public long PC;         // program counter, has to be incremented manually
        public Dictionary<string, object> MEM; // can be used to store state between calls

        public Input() : this(new List<bool>(), 0, new Dictionary<string, object>()) { }
        public Input(List<bool> l, long num, Dictionary<string, object> mem)
        {
            I = l;
            O = new List<bool>();
            PC = num;
            MEM = mem;
        }
        // returns the state of the input at 'i' 
        // returns false when out of range
        public bool Get(int i)
        {
            return i < I.Count ? I[i] : false;
        }

        // sets the state of the output at 'o'
        // adds more outputs if necessary
        public void Set(int i, bool val)
        {
            while (i >= O.Count)
            {
                O.Add(false);
            }
            O[i] = val;
        }

        // converts a List of bools to a uint
        public uint ToInt(List<bool> l)
        {
            uint num = 0;
            for (int j = l.Count - 1; j >= 0; j--)
            {
                num <<= 1;
                num += l[j] ? (uint)1 : 0;
            }
            return num;
        }

        public List<bool> FromInt(long num)
        {
            return FromInt((uint)num);
        }
        // converts a int to a list of bools
        public List<bool> FromInt(uint num)
        {
            int j = 0;
            List<bool> l = new();
            while (num != 0)
            {
                bool b = (num & (uint)1) == (uint)1;
                l.Add(b);
                num >>= 1;
                j++;
            }
            return l;
        }

        // converts a list to a array
        // usefull because List is not available in the script
        public bool[] ToArray(List<bool> list)
        {
            return list.ToArray<bool>();
        }

        // converts array back to list to use as output
        public List<bool> FromArray(bool[] arr)
        {
            return arr.ToList();
        }

        // saves a object to memory
        public void Save(string name, object value)
        {
            if (MEM.ContainsKey(name))
                MEM[name] = value;
            else
                MEM.Add(name, value);
        }

        // loads a object from memory
        public object Load(string name)
        {
            return MEM[name];
        }
    }

    // used to store the script and to instantiate components
    public class CCode : ComponentCreator
    {
        private readonly Script<List<bool>> _script;
        private readonly string _ext;      // file extension


        public CCode(string filename, string txt)
        {
            _script = LoadCs(txt);
            Name = filename;
            _ext = Path.GetExtension(filename);
            OnColor = Color.GRAY;
            OffColor = Color.GRAY;
        }
        // loads a script from file storage
        private static Script<List<bool>> LoadCs(string txt)
        {
            var opt = ScriptOptions.Default;
            opt.AddReferences(typeof(List<bool>).Assembly, typeof(Input).Assembly);
            opt.AddImports("System");
            var script = CSharpScript.Create<List<bool>>(txt, opt, typeof(Input));
            script.Compile(); // compile on load for faster further use
            return script;
        }

        // runs the script with the given inputs and returns the output of the script
        public List<bool> Run(Input input)
        {
            ScriptState<List<bool>> state = _script.RunAsync(input).Result;
            return state.ReturnValue;
        }

        // creates a component with this script
        public override Component CreateComponent(State state)
        {
            if (_ext == ".cpl")
                return new CondComp(this, state);
            return new ProgComp(this, state);
        }

        public override void Draw(int x, int y, int gridsize, bool state)
        {
            Color color = state ? OnColor : OffColor;
            Raylib.DrawRectangle(x, y, gridsize, gridsize, color);
            Raylib.DrawText(Name[0].ToString(), x + gridsize / 3, y, gridsize, Color.BLACK);
        }
    }

}