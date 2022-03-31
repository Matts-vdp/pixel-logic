using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;


namespace game {
    public class Input {

        public List<bool> i;
        public List<bool> o;
        public long PC;
        public Dictionary<string,object> MEM;
        public Input(List<bool> l, long num, Dictionary<string, object> mem) {
            i = l;
            o = new List<bool>();
            PC = num;
            MEM = mem;
        }
        public bool get(int i){
            if (i < this.i.Count){
                return this.i[i];
            }
            return false;
        }

        public void set(int i, bool val){
            while (i >= o.Count){
                o.Add(false);
            }
            o[i] = val;
        }
        public uint toInt(List<bool> l){
            uint num = 0;
            for(int j=l.Count-1; j>=0; j--){
                num = num<<1;
                num += l[j]? (uint)1: 0;
            }
            return num;
        }

        public List<bool> fromInt(uint num) {
            int j=0;
            List<bool> l = new List<bool>();
            while (num != 0) {
                bool b = (num&(uint)1)==(uint)1;
                l.Add(b);
                num = num>>1;
                j++;
            }
            return l;
        }
        public bool[] toArray(List<bool> list) {
            return list.ToArray<bool>();
        }

        public List<bool> fromArray(bool[] arr){
            return arr.ToList<bool>();
        }
        public void save(string name, object value){
            if (MEM.ContainsKey(name))
                MEM[name] = value;
            else
                MEM.Add(name, value);
        }
        public object load(string name) {
            return MEM[name];
        }
    }

    public class Codes {
        public static Dictionary<int, CCode> codeMap = new Dictionary<int, CCode>();
        public static void add(string filename) {
            ComponentFactory.items.Add(Path.GetFileNameWithoutExtension(filename));
            codeMap.Add(ComponentFactory.items.Count, new CCode(filename));
        }
    }

    public class CCode {
        public Script<List<bool>>? script;
        public string file;
        public string ext;
        private long PC = 0;

        private Dictionary<string,object> mem;
        public ScriptState<List<bool>>? state;
        public CCode(string filename){
            script = loadCs("customComponents/"+filename);
            file = filename;
            ext = Path.GetExtension(filename);
            mem = new Dictionary<string, object>();
        }
        public Script<List<bool>>? loadCs(string filename) {
            string txt = File.ReadAllText(filename);
            var opt = ScriptOptions.Default;
            // opt.WithMetadataResolver(ScriptMetadataResolver.Default.WithBaseDirectory("./"));
            opt.AddReferences(typeof(List<>).Assembly, typeof(Input).Assembly);
            opt.AddImports("System");
            var script = CSharpScript.Create<List<bool>>(txt, opt, typeof(Input));
            script.Compile();
            
            return script;
        }
        public List<bool> run(List<bool> inputs) {
            if (script == null) {
                return new List<bool>();
            }
            Input param = new Input(inputs, PC, mem);
            state = script.RunAsync(param).Result;
            PC = param.PC;
            mem = param.MEM;
            return state.ReturnValue;
        }
    }

}