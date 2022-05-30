using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;


namespace game
{
    // used by the custom script to interact with the logic
    // the attributes and functions of this class can be used in the script
    public class Input
    {

        public List<bool> i;    //input connections
        public List<bool> o;    // output connections
        public long PC;         // program counter, has to be incremented manually
        public Dictionary<string, object> MEM; // can be used to store state between calls
        
        public Input(): this(new List<bool>(), 0, new Dictionary<string, object>()){}
        public Input(List<bool> l, long num, Dictionary<string, object> mem)
        {
            i = l;
            o = new List<bool>();
            PC = num;
            MEM = mem;
        }
        // returns the state of the input at 'i' 
        // returns false when out of range
        public bool get(int i)
        {
            if (i < this.i.Count)
            {
                return this.i[i];
            }
            return false;
        }

        // sets the state of the output at 'o'
        // adds more outputs if necessary
        public void set(int i, bool val)
        {
            while (i >= o.Count)
            {
                o.Add(false);
            }
            o[i] = val;
        }

        // converts a List of bools to a uint
        public uint toInt(List<bool> l)
        {
            uint num = 0;
            for (int j = l.Count - 1; j >= 0; j--)
            {
                num = num << 1;
                num += l[j] ? (uint)1 : 0;
            }
            return num;
        }

        public List<bool> fromInt(long num) {
            return fromInt((uint) num);
        }
        // converts a int to a list of bools
        public List<bool> fromInt(uint num)
        {
            int j = 0;
            List<bool> l = new List<bool>();
            while (num != 0)
            {
                bool b = (num & (uint)1) == (uint)1;
                l.Add(b);
                num = num >> 1;
                j++;
            }
            return l;
        }

        // converts a list to a array
        // usefull because List is not available in the script
        public bool[] toArray(List<bool> list)
        {
            return list.ToArray<bool>();
        }

        // converts array back to list to use as output
        public List<bool> fromArray(bool[] arr)
        {
            return arr.ToList<bool>();
        }

        // saves a object to memory
        public void save(string name, object value)
        {
            if (MEM.ContainsKey(name))
                MEM[name] = value;
            else
                MEM.Add(name, value);
        }

        // loads a object from memory
        public object load(string name)
        {
            return MEM[name];
        }
    }

    // used to store the script and to instantiate components
    public class CCode : CustomComponentCreator
    {
        public Script<List<bool>>? script;
        public string ext;      // file extension
        
        
        public CCode(string filename)
        {
            script = loadCs("saves/customCode/" + filename);
            name = filename;
            ext = Path.GetExtension(filename);
        }
        // loads a script from file storage
        public Script<List<bool>>? loadCs(string filename)
        {
            string txt = File.ReadAllText(filename);
            var opt = ScriptOptions.Default;
            opt.AddReferences(typeof(List<bool>).Assembly, typeof(Input).Assembly);
            opt.AddImports("System");
            var script = CSharpScript.Create<List<bool>>(txt, opt, typeof(Input));
            script.Compile(); // compile on load for faster further use
            return script;
        }

        // runs the script with the given inputs and returns the output of the script
        public List<bool> run(Input input)
        {
            if (script == null)
            {
                return new List<bool>();
            }
            ScriptState<List<bool>> state = script.RunAsync(input).Result;
            return state.ReturnValue;
        }

        // creates a component with this script
        public override Component toComponent(ComponentList list, int type)
        {
            if (ext == ".cpl")
                return new CondComp(type, list);
            return new ProgComp(type, list);
        }
    }

}