using Xunit;
using Game.Components;

namespace pixel_logic.Tests;

public class GridTest
{
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
        int[,] mat = {
            {0,i,i,i},
            {0,i,0,0},
            {0,i,i,i},
        };

        int[,] result = {
            {0,1,1,1},
            {0,1,0,0},
            {0,1,1,1}
        };

        Grid g = new Grid(3,4,mat);
        int[,] labels = g.connectedComponents();
        Assert.Equal(result, labels);
    }
}