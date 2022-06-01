using Raylib_cs;

namespace Game.Components
{

    public abstract class ComponentCreator: CustomCreator
    {
        public abstract Component createComponent(State state);
    }
    // represents a class that can be used as a Custom Component
    public class BasicComponentCreator: ComponentCreator
    {
        public Func<State, Component> newComponent;
        public BasicComponentCreator(string name, Color offColor, Color onColor, Func<State, Component> func){
            newComponent = func;
            this.offColor = offColor;
            this.onColor = onColor;
            this.name = name;
        }
        public override Component createComponent(State state)
        {
            return newComponent(state);
        }
    }
}