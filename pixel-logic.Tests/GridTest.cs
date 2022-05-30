using Xunit;
using Game.Components;

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
}