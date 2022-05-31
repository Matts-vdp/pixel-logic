using Raylib_cs;

namespace Game.Components
{
    // represents a class that can be used as a Custom Component
    public class ConnectionCreator: CustomCreator
    {
        public Func<Pos, State, Connection> newConnection;
        public ConnectionCreator(string name, Color offColor, Color onColor, Func<Pos, State, Connection> func){
            newConnection = func;
            this.offColor = offColor;
            this.onColor = onColor;
            this.name = name;
        }
        public Connection createConnection(Pos p, State state)
        {
            return newConnection(p, state);
        }
    }
}