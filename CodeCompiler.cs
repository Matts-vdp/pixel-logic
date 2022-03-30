using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace game {
    public class Input {

        public List<bool> i;
        public List<bool> o;
        public int cnt;
        public Input(List<bool> l, int num) {
            i = l;
            o = new List<bool>();
            cnt = num;
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
    }

    public class Codes {
        public static Dictionary<int, CCode> codeMap = new Dictionary<int, CCode>();
        public static void add(string filename) {
            ComponentFactory.items.Add(filename);
            codeMap.Add(ComponentFactory.items.Count, new CCode(filename));
        }
    }

    public class CCode {
        public Script<List<bool>>? script;
        public string file;
        private int cnt = 0;
        public ScriptState<List<bool>>? state;
        public CCode(string filename){
            script = loadCs(filename);
            file = filename;
        }
        public Script<List<bool>>? loadCs(string filename) {
            string txt = File.ReadAllText(filename + ".script");
            var opt = ScriptOptions.Default;
            opt.AddReferences(typeof(List<bool>).Assembly, typeof(Input).Assembly);
            opt.AddImports("System");
            var script = CSharpScript.Create<List<bool>>(txt, opt, typeof(Input));
            script.Compile();
            
            return script;
        }
        public List<bool> run(List<bool> inputs) {
            if (script == null) {
                return new List<bool>();
            }
            Input param = new Input(inputs, cnt);
            state = script.RunAsync(param).Result;
            cnt++;
            return state.ReturnValue;
        }
    }

}