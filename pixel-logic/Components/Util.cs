namespace Game.Components
{
    // struct to contain a 2d coordinate
    public struct Pos
    {
        public int x { get; set; }
        public int y { get; set; }
        public Pos(int X, int Y)
        {
            x = X;
            y = Y;
        }
    }

    public class State
    {
        private bool[,] state;
        public State(int w, int h)
        {
            state = new bool[h,w];
        }
        public void setState(Pos p, bool newState) 
        {
            state[p.y, p.x] = newState;
        }
        public bool getState(Pos p) 
        {
            return state[p.y, p.x];
        }
    }

    
}