using Xunit;
using game;

namespace pixel_logic.Tests;

public class MainTest
{
    [Fact]
    public void Test1()
    {
        Assert.True(Game.GRIDSIZE == 32);
    }
}