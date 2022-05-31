using Xunit;
using Game.Components;

namespace pixel_logic.Tests;

public class ComponentsTest
{
    [Fact]
    public void ConnectionisFull()
    {
        State state = new State(10,10);
        Connection conn = new OutConnection(new Pos(0,0), state);
        Assert.False(conn.isFull());
        conn.addOther(new OrComp(state));
        Assert.False(conn.isFull());
        conn.addWire(new OrComp(state));
        Assert.True(conn.isFull());
    }
    
    [Fact]
    public void ConnectionisChanged()
    {
        State state = new State(10,10);
        Connection conn = new OutConnection(new Pos(), state);
        Assert.True(conn.isChanged());
        Assert.False(conn.isActive());
        Assert.False(conn.isChanged());
        conn.setActive(true);
        Assert.True(conn.isChanged());
        Assert.True(conn.isActive());
        Assert.False(conn.isChanged());
        conn.setActive(true);
        Assert.False(conn.isChanged());
        Assert.True(conn.isActive());
    }
    
    [Fact]
    public void Wire()
    {
        State state = new State(10,10);
        Component wire = new WireComp(state);
        wire.add(new Pos(0,0));
        Connection input = new OutConnection(new Pos(0,1), state);
        Connection output = new InConnection(new Pos(0,2), state);
        input.addWire(wire);
        output.addWire(wire);
        Assert.False(input.isActive());
        Assert.False(output.isActive());
        input.setActive(true);
        Assert.False(output.isActive());
        wire.update();
        Assert.True(output.isActive());
    }
    [Fact]
    public void And()
    {
        State state = new State(10,10);
        Component comp = new AndComp(state);
        comp.add(new Pos(0,0));
        Connection input = new InConnection(new Pos(0,1), state);
        Connection input2 = new InConnection(new Pos(0,2), state);
        Connection output = new OutConnection(new Pos(0,3), state);
        input.addOther(comp);
        input2.addOther(comp);
        output.addOther(comp);
        comp.update();
        Assert.False(output.isActive());
        input.setActive(true);
        comp.update();
        Assert.False(output.isActive());
        input2.setActive(true);
        comp.update();
        Assert.True(output.isActive());
    }
    [Fact]
    public void Not()
    {
        State state = new State(10,10);
        Component comp = new NotComp(state);
        comp.add(new Pos(0,0));
        Connection input = new InConnection(new Pos(0,1), state);
        Connection output = new OutConnection(new Pos(0,2), state);
        input.addOther(comp);
        output.addOther(comp);
        comp.update();
        Assert.True(output.isActive());
        input.setActive(true);
        comp.update();
        Assert.False(output.isActive());
    }

    [Fact]
    public void Or()
    {
        State state = new State(10,10);
        Component comp = new OrComp(state);
        comp.add(new Pos(0,0));
        Connection input = new InConnection(new Pos(0,1), state);
        Connection input2 = new InConnection(new Pos(0,2), state);
        Connection output = new OutConnection(new Pos(0,3), state);
        input.addOther(comp);
        input2.addOther(comp);
        output.addOther(comp);
        comp.update();
        Assert.False(output.isActive());
        input.setActive(true);
        comp.update();
        Assert.True(output.isActive());
        input2.setActive(true);
        comp.update();
        Assert.True(output.isActive());
    }

    [Fact]
    public void Exor()
    {
        State state = new State(10,10);
        Component comp = new XorComp(state);
        comp.add(new Pos(0,0));
        Connection input = new InConnection(new Pos(0,1), state);
        Connection input2 = new InConnection(new Pos(0,2), state);
        Connection output = new OutConnection(new Pos(0,3), state);
        input.addOther(comp);
        input2.addOther(comp);
        output.addOther(comp);
        comp.update();
        Assert.False(output.isActive());
        input.setActive(true);
        comp.update();
        Assert.True(output.isActive());
        input2.setActive(true);
        comp.update();
        Assert.False(output.isActive());
    }

    [Fact]
    public void Battery()
    {
        State state = new State(10,10);
        Component comp = new BatteryComp(state);
        comp.add(new Pos(0,0));
        Connection output = new OutConnection(new Pos(0,0), state);
        output.addOther(comp);
        comp.update();
        Assert.True(output.isActive());
    }
    [Fact]
    public void FlipFlop()
    {
        State state = new State(10,10);
        Component comp = new FlipFlopComp(state);
        comp.add(new Pos(0,0));
        Connection input = new InConnection(new Pos(0,1), state);
        Connection clock = new ClockIn(new Pos(0,2), state);
        Connection output = new OutConnection(new Pos(0,3), state);
        input.addOther(comp);
        clock.addOther(comp);
        output.addOther(comp);
        comp.update();
        Assert.False(output.isActive());
        input.setActive(true);
        comp.update();
        Assert.False(output.isActive());
        clock.setActive(true);
        comp.update();
        Assert.False(output.isActive());
        comp.update();
        Assert.True(output.isActive());
    }
}