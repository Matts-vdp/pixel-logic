using Raylib_cs;

namespace Game.Components
{
    // represents a class that can be used as a Custom Component
    public class ConnectionCreator : CustomCreator
    {
        public Func<Pos, State, Connection> NewConnection;
        public ConnectionCreator(string name, Color offColor, Color onColor, Func<Pos, State, Connection> func)
        {
            NewConnection = func;
            this.OffColor = offColor;
            this.OnColor = onColor;
            this.Name = name;
        }
        public Connection CreateConnection(Pos p, State state)
        {
            return NewConnection(p, state);
        }
    }
}