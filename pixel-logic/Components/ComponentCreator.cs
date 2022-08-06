using Raylib_cs;

namespace Game.Components
{
    // used to define a common Creator for all basic and custom components
    public abstract class ComponentCreator : CustomCreator
    {
        public abstract Component CreateComponent(State state);
    }

    // Used to create basic components
    public class BasicComponentCreator : ComponentCreator
    {
        public Func<State, Component> NewComponent;
        public BasicComponentCreator(string name, Color offColor, Color onColor, Func<State, Component> func)
        {
            NewComponent = func;
            this.OffColor = offColor;
            this.OnColor = onColor;
            this.Name = name;
        }
        public override Component CreateComponent(State state)
        {
            return NewComponent(state);
        }
    }
}