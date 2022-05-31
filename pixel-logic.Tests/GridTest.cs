using Xunit;
using Game.Components;
using System.Numerics;

namespace pixel_logic.Tests;

public class GridTest
{
    [Fact]
    public void Constructor1()
    {
        Grid g = new Grid(5, 5);
    }

    [Theory]
    [InlineData(300,30,0,10)]
    [InlineData(0,20,0,0)]
    [InlineData(0,10,100,10)]
    public void toGrid(float pos, int size, int offset, int result)
    {
        int gridPos = Grid.toGrid(pos, size, offset);
        Assert.True(gridPos == result);
    }

    [Theory]
    [InlineData(0,0, (int)types.OUT, false)]
    [InlineData(0,1, (int)types.OUT, true)]
    [InlineData(0,0, (int)types.IN, true)]
    [InlineData(0,100, (int)types.OUT, false)]
    public void add(int x, int y, int t, bool result) 
    {
        Grid g = new Grid(5, 5);
        bool change = g.add(new Vector2(0,0), (int)types.OUT, 1,0,0);
        Assert.True(change);
        change = g.add(new Vector2(x,y), t, 1,0,0);
        Assert.Equal(change, result);
    }
    [Theory]
    [InlineData(0,0, true)]
    [InlineData(0,1, false)]
    [InlineData(0,100, false)]
    public void del(int x, int y, bool result) 
    {
        Grid g = new Grid(5, 5);
        bool change = g.add(new Vector2(0,0), (int)types.OUT, 1,0,0);
        Assert.True(change);
        change = g.del(new Vector2(x,y), 1,0,0);
        Assert.Equal(change, result);
    }

    
    [Fact]
    public void buildComponents()
    {
        Grid g = new Grid(10, 10);
        g.add(new Vector2(0,0), (int)types.OUT, 1,0,0);
        g.add(new Vector2(0,1), (int)types.WIRE, 1,0,0);
        g.add(new Vector2(0,2), (int)types.IN, 1,0,0);
        g.add(new Vector2(0,3), (int)types.OR, 1,0,0);
        g.add(new Vector2(0,4), (int)types.OR, 1,0,0);
        g.add(new Vector2(0,5), (int)types.OUT, 1,0,0);
        g.add(new Vector2(0,6), (int)types.WIRE, 1,0,0);
        g.add(new Vector2(0,7), (int)types.IN, 1,0,0);
        Circuit circuit = g.buildObjects();
        Component comp = circuit.toComponent(g.state);
        State state = new State(5,5);
        comp.add(new Pos(0,0));
        Connection output = new OutConnection(new Pos(0,1), state);
        Connection input = new InConnection(new Pos(0,2), state);
        input.addOther(comp);
        output.addOther(comp);
        comp.update();
        Assert.False(output.isActive());
        input.setActive(true);
        comp.update();
        comp.update();
        Assert.True(output.isActive());
    }
}