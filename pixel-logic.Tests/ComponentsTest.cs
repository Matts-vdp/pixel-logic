using Game.Components;

using Xunit;

namespace Pixel_logic.Tests;

public class ComponentsTest
{
    [Fact]
    public void ConnectionisFull()
    {
        State state = new(10, 10);
        Connection conn = new OutConnection(new Pos(0, 0), state);
        Assert.False(conn.IsFull());
        conn.AddOther(new OrComp(state));
        Assert.False(conn.IsFull());
        conn.AddWire(new OrComp(state));
        Assert.True(conn.IsFull());
    }

    [Fact]
    public void ConnectionisChanged()
    {
        State state = new(10, 10);
        Connection conn = new OutConnection(new Pos(), state);
        Assert.True(conn.IsChanged());
        Assert.False(conn.IsActive()[0]);
        Assert.False(conn.IsChanged());
        conn.SetActive(Value.True());
        Assert.True(conn.IsChanged());
        Assert.True(conn.IsActive()[0]);
    }

    [Fact]
    public void Wire()
    {
        State state = new(10, 10);
        Component wire = WireComp.NewComponent(state);
        wire.Add(new Pos(0, 0));
        Connection input = new OutConnection(new Pos(0, 1), state);
        Connection output = new InConnection(new Pos(0, 2), state);
        input.AddWire(wire);
        output.AddWire(wire);
        Assert.False(input.IsActive()[0]);
        Assert.False(output.IsActive()[0]);
        input.SetActive(Value.True());
        Assert.False(output.IsActive()[0]);
        wire.Update();
        Assert.True(output.IsActive()[0]);
    }
    [Fact]
    public void And()
    {
        State state = new(10, 10);
        Component comp = AndComp.NewComponent(state);
        comp.Add(new Pos(0, 0));
        Connection input = new InConnection(new Pos(0, 1), state);
        Connection input2 = new InConnection(new Pos(0, 2), state);
        Connection output = new OutConnection(new Pos(0, 3), state);
        input.AddOther(comp);
        input2.AddOther(comp);
        output.AddOther(comp);
        comp.Update();
        Assert.False(output.IsActive()[0]);
        input.SetActive(Value.True());
        comp.Update();
        Assert.False(output.IsActive()[0]);
        input2.SetActive(Value.True());
        comp.Update();
        Assert.True(output.IsActive()[0]);
    }
    [Fact]
    public void Not()
    {
        State state = new(10, 10);
        Component comp = NotComp.NewComponent(state);
        comp.Add(new Pos(0, 0));
        Connection input = new InConnection(new Pos(0, 1), state);
        Connection output = new OutConnection(new Pos(0, 2), state);
        input.AddOther(comp);
        output.AddOther(comp);
        comp.Update();
        Assert.True(output.IsActive()[0]);
        input.SetActive(Value.True());
        comp.Update();
        Assert.False(output.IsActive()[0]);
    }

    [Fact]
    public void Or()
    {
        State state = new(10, 10);
        Component comp = OrComp.NewComponent(state);
        comp.Add(new Pos(0, 0));
        Connection input = new InConnection(new Pos(0, 1), state);
        Connection input2 = new InConnection(new Pos(0, 2), state);
        Connection output = new OutConnection(new Pos(0, 3), state);
        input.AddOther(comp);
        input2.AddOther(comp);
        output.AddOther(comp);
        comp.Update();
        Assert.False(output.IsActive()[0]);
        input.SetActive(Value.True());
        comp.Update();
        Assert.True(output.IsActive()[0]);
        input2.SetActive(Value.True());
        comp.Update();
        Assert.True(output.IsActive()[0]);
    }

    [Fact]
    public void Exor()
    {
        State state = new(10, 10);
        Component comp = XorComp.NewComponent(state);
        comp.Add(new Pos(0, 0));
        Connection input = new InConnection(new Pos(0, 1), state);
        Connection input2 = new InConnection(new Pos(0, 2), state);
        Connection output = new OutConnection(new Pos(0, 3), state);
        input.AddOther(comp);
        input2.AddOther(comp);
        output.AddOther(comp);
        comp.Update();
        Assert.False(output.IsActive()[0]);
        input.SetActive(Value.True());
        comp.Update();
        Assert.True(output.IsActive()[0]);
        input2.SetActive(Value.True());
        comp.Update();
        Assert.False(output.IsActive()[0]);
    }

    [Fact]
    public void Battery()
    {
        State state = new(10, 10);
        Component comp = BatteryComp.NewComponent(state);
        comp.Add(new Pos(0, 0));
        Connection output = new OutConnection(new Pos(0, 0), state);
        output.AddOther(comp);
        comp.Update();
        Assert.True(output.IsActive()[0]);
    }
    [Fact]
    public void FlipFlop()
    {
        State state = new(10, 10);
        Component comp = FlipFlopComp.NewComponent(state);
        comp.Add(new Pos(0, 0));
        Connection input = new InConnection(new Pos(0, 1), state);
        Connection clock = new ClockIn(new Pos(0, 2), state);
        Connection output = new OutConnection(new Pos(0, 3), state);
        input.AddOther(comp);
        clock.AddOther(comp);
        output.AddOther(comp);
        comp.Update();
        Assert.False(output.IsActive()[0]);
        input.SetActive(Value.True());
        comp.Update();
        Assert.False(output.IsActive()[0]);
        clock.SetActive(Value.True());
        comp.Update();
        Assert.False(output.IsActive()[0]);
        comp.Update();
        Assert.True(output.IsActive()[0]);
    }

    [Fact]
    public void Button()
    {
        State state = new(10, 10);
        ButtonComp comp = (ButtonComp)ButtonComp.NewComponent(state);
        comp.Add(new Pos(0, 0));
        Connection output = new OutConnection(new Pos(0, 3), state);
        output.AddOther(comp);
        comp.Update();
        Assert.False(output.IsActive()[0]);
        comp.Toggle();
        comp.Update();
        Assert.True(output.IsActive()[0]);
        comp.Toggle();
        comp.Update();
        Assert.False(output.IsActive()[0]);
    }
}