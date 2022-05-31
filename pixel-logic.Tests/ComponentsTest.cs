using Xunit;
using Game.Components;

namespace pixel_logic.Tests;

public class ComponentsTest
{
    [Fact]
    public void ConnectionisFull()
    {
        Connection conn = new OutConnection(new Pos());
        Assert.False(conn.isFull());
        conn.addOther(new OrComp());
        Assert.False(conn.isFull());
        conn.addWire(new OrComp());
        Assert.True(conn.isFull());
    }
    
    [Fact]
    public void ConnectionisChanged()
    {
        Connection conn = new OutConnection(new Pos());
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
        Component wire = new WireComp();
        Connection input = new OutConnection(new Pos(0,0));
        Connection output = new InConnection(new Pos(0,0));
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
        Component comp = new AndComp();
        Connection input = new InConnection(new Pos(0,0));
        Connection input2 = new InConnection(new Pos(0,0));
        Connection output = new OutConnection(new Pos(0,0));
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
        Component comp = new NotComp();
        Connection input = new InConnection(new Pos(0,0));
        Connection output = new OutConnection(new Pos(0,0));
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
        Component comp = new OrComp();
        Connection input = new InConnection(new Pos(0,0));
        Connection input2 = new InConnection(new Pos(0,0));
        Connection output = new OutConnection(new Pos(0,0));
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
        Component comp = new XorComp();
        Connection input = new InConnection(new Pos(0,0));
        Connection input2 = new InConnection(new Pos(0,0));
        Connection output = new OutConnection(new Pos(0,0));
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
        Component comp = new BatteryComp();
        Connection output = new OutConnection(new Pos(0,0));
        output.addOther(comp);
        comp.update();
        Assert.True(output.isActive());
    }
    [Fact]
    public void FlipFlop()
    {
        Component comp = new FlipFlopComp();
        Connection input = new InConnection(new Pos(0,0));
        Connection clock = new ClockIn(new Pos(0,0));
        Connection output = new OutConnection(new Pos(0,0));
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
        Assert.True(output.isActive());
    }
}