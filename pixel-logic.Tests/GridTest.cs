using Game.Components;

using Xunit;

namespace Pixel_logic.Tests;

public class GridTest
{
    [Fact]
    public void Set()
    {
        Grid g = new(5, 5);
        bool change = g.Set(0, 0, 10);
        Assert.Equal(10, g[0, 0]);
        Assert.True(change);
        change = g.Set(0, 0, 10);
        Assert.Equal(10, g[0, 0]);
        Assert.False(change);
        change = g.Set(-1, -1, 10);
        Assert.False(change);
        change = g.Set(10, 5, 10);
        Assert.False(change);
    }

    [Fact]
    public void Clear()
    {
        Grid g = new(5, 5);
        g.Set(0, 0, 10);
        g.Clear();
        Assert.Equal(0, g[0, 0]);
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
        int[,] result = Grid.ChangeLabel(i, 1, mat, 3, 4);
        Assert.Equal(truth, result);
    }

    [Fact]
    public void ConnectedComponents1()
    {
        Grid g = new(6, 6);
        g.Set(0, 0, (int)Types.WIRE);
        g.Set(0, 1, (int)Types.WIRE);
        g.Set(1, 0, (int)Types.WIRE);
        g.Set(0, 2, (int)Types.IN);
        g.Set(0, 3, (int)Types.WIRE);
        g.Set(0, 4, (int)Types.WIRE);
        g.Set(0, 5, (int)Types.CROSS);
        int[,] labels = g.ConnectedComponents();
        Assert.Equal(1, labels[0, 0]);
        Assert.Equal(1, labels[0, 1]);
        Assert.Equal(1, labels[1, 0]);
        Assert.Equal(-1, labels[0, 2]);
        Assert.Equal(2, labels[0, 3]);
        Assert.Equal(2, labels[0, 4]);
        Assert.Equal(-2, labels[0, 5]);
    }
    [Fact]
    public void ConnectedComponents2()
    {
        int i = (int)Types.WIRE;
        int o = (int)Types.AND;
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

        Grid g = new(3, 4, mat);
        int[,] labels = g.ConnectedComponents();
        Assert.Equal(result, labels);
    }

    [Fact]
    public void Copy()
    {
        int[,] mat = {
            {1,2},
            {0,3},
        };
        Grid g = new(2, 2, mat);
        Grid result = g.Copy(-1, -1, 2, 2);
        int[,] truth = {
            {0,0,0,0},
            {0,1,2,0},
            {0,0,3,0},
            {0,0,0,0}
        };
        Assert.Equal(truth, result.Matrix);
    }
    [Fact]
    public void Paste()
    {
        int[,] mat = {
            {1,2},
            {0,3},
        };
        Grid g = new(2, 2, mat);
        Grid o = new(2, 2);
        o.Paste(g, 1, 1);
        o.Paste(g, -1, -1);
        int[,] truth = {
            {3,0},
            {0,1},
        };
        Assert.Equal(truth, o.Matrix);
    }
}