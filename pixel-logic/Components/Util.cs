namespace Game.Components
{
    // struct to contain a 2d coordinate
    public struct Pos
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Pos(int x, int y)
        {
            X = x;
            Y = y;
        }
    }


    public struct Value
    {
        public readonly List<bool> Values;
        public bool this[int i]
        {
            get
            {
                if (i < Values.Count)
                    return Values[i];
                return false;
            }
            set
            {
                while (i >= Values.Count)
                    Values.Add(false);
                Values[i] = value;
            }
        }

        public readonly int Count
        {
            get
            {
                return Values.Count;
            }
        }

        public Value()
        {
            Values = new();
        }

        public static Value True()
        {
            Value newValue = new();
            newValue[0] = true;
            return newValue;
        }

        public void Reset()
        {
            Values.Clear();
            Values.Add(false);
        }
        public void Add(Value value)
        {
            Values.AddRange(value.Values);
        }
    }

    public class State
    {
        private readonly Value[,] _state;
        public Dictionary<Pos, string> DrawText = new();

        public State(int w, int h)
        {
            _state = new Value[w, h];
            for (int y=0; y<h; y++)
                for (int x=0; x<w; x++)
                    _state[x,y] = new();
        }
        public void SetState(Pos p, Value newState)
        {
            lock (this)
            {
                _state[p.X, p.Y] = newState;
            }
        }
        public void ResetState(Pos p)
        {
            lock (this)
            {
                _state[p.X, p.Y].Reset();
            }
        }
        public bool GetBoolState(Pos p)
        {
            lock (this)
            {
                Value val = _state[p.X, p.Y];
                for (int i=0; i<val.Count; i++)
                    if (val[i]) return true;
                return false;
            }
        }
        public Value GetState(Pos p)
        {
            lock (this)
            {
                return _state[p.X, p.Y];
            }
        }

        public void SetText(List<Pos> positions, string text)
        {
            lock (this)
            {
                foreach(Pos p in positions)
                {
                    DrawText[p] = text;
                }
            }
        }
    }

    public interface IFile
    {
        public string ReadAllText(string filename);
        public void WriteAllTextAsync(string filename, string txt);

    }

}