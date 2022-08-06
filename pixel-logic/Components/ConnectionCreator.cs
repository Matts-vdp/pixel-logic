using Raylib_cs;

namespace Game.Components
{
    // used to create a new Connection
    public class ConnectionCreator : CustomCreator
    {
        public Func<Pos, State, Connection> NewConnection;
        public ConnectionCreator(string name, Color offColor, Color onColor, Func<Pos, State, Connection> func)
        {
            NewConnection = func;
            OffColor = offColor;
            OnColor = onColor;
            Name = name;
        }
        public Connection CreateConnection(Pos p, State state)
        {
            return NewConnection(p, state);
        }
    }
}