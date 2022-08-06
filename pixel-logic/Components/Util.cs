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

    // Contains the state of all locations in the grid
    // also used to display a text on top of the blocks
    public class State
    {
        private readonly bool[,] _state;
        public Dictionary<Pos, string> DrawText = new();

        public State(int w, int h)
        {
            lock(this) 
            {
                _state = new bool[w, h];
            }
        }
        public void SetState(Pos p, bool newState)
        {
            lock(this) 
            {
                _state[p.X, p.Y] = newState;
            }
        }
        public bool GetState(Pos p)
        {
            lock(this) 
            {
                return _state[p.X, p.Y];
            }
        }

        public void SetText(List<Pos> positions, string text)
        {
            lock(this) 
            {
                foreach(Pos p in positions)
                {
                    DrawText[p] = text;
                }
            }
        }
    }

    // common interface to abstract filesystem
    public interface IFile
    {
        public string ReadAllText(string filename);
        public void WriteAllTextAsync(string filename, string txt);

    }

}