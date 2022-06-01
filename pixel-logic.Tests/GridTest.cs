using Xunit;
using Game.Components;

namespace pixel_logic.Tests;

public class GridTest
{
    [Fact]
    public void Set()
    {
        Grid g = new Grid(5,5);
        bool change = g.set(0,0,10);
        Assert.Equal(10, g[0,0]);
        Assert.True(change);
        change = g.set(0,0,10);
        Assert.Equal(10, g[0,0]);
        Assert.False(change);
        change = g.set(-1,-1,10);
        Assert.False(change);
        change = g.set(10,5,10);
        Assert.False(change);
    }

    [Fact]
    public void Clear()
    {
        Grid g = new Grid(5,5);
        g.set(0,0,10);
        g.clear();
        Assert.Equal(0, g[0,0]);
    }

    [Fact]
    public void ChangeLabel()
    {
        int i = 3;
        int[,] mat = {
            {6,i,i,i},
            {4,i,0,1},
            {4,i,i,i},
        };
        int[,] truth = {
            {6,1,1,1},
            {4,1,0,1},
            {4,1,1,1}
        };
        int[,] result = Grid.changeLabel(i, 1, mat, 3,4);
        Assert.Equal(truth, result);
    }

    [Fact]
    public void ConnectedComponents1()
    {
        Grid g = new Grid(6,6);
        g.set(0,0, (int)types.WIRE);
        g.set(0,1, (int)types.WIRE);
        g.set(1,0, (int)types.WIRE);
        g.set(0,2, (int)types.IN);
        g.set(0,3, (int)types.WIRE);
        g.set(0,4, (int)types.WIRE);
        g.set(0,5, (int)types.CROSS);
        int[,] labels = g.connectedComponents();
        Assert.Equal(1, labels[0,0]);
        Assert.Equal(1, labels[0,1]);
        Assert.Equal(1, labels[1,0]);
        Assert.Equal(-1, labels[0,2]);
        Assert.Equal(2, labels[0,3]);
        Assert.Equal(2, labels[0,4]);
        Assert.Equal(-2, labels[0,5]);
    }
    [Fact]
    public void ConnectedComponents2()
    {
        int i = (int)types.WIRE;
        int o = (int)types.AND;
        int[,] mat = {
            {0,i,o,i},
            {0,i,o,i},
            {0,i,i,i},
        };

        int[,] result = {
            {0,1,2,1},
            {0,1,2,1},
            {0,1,1,1}
        };

        Grid g = new Grid(3,4,mat);
        int[,] labels = g.connectedComponents();
        Assert.Equal(result, labels);
    }

    [Fact]
    public void Copy()
    {
        int[,] mat = {
            {1,2},
            {0,3},
        };
        Grid g = new Grid(2,2, mat);
        Grid result = g.copy(-1,-1,2,2);
        int[,] truth = {
            {0,0,0,0},
            {0,1,2,0},
            {0,0,3,0},
            {0,0,0,0}
        };
        Assert.Equal(truth, result.grid);
    }
    [Fact]
    public void Paste()
    {
        int[,] mat = {
            {1,2},
            {0,3},
        };
        Grid g = new Grid(2,2, mat);
        Grid o = new Grid(2,2);
        o.paste(g, 1,1);
        o.paste(g, -1,-1);
        int[,] truth = {
            {3,0},
            {0,1},
        };
        Assert.Equal(truth, o.grid);
    }
}