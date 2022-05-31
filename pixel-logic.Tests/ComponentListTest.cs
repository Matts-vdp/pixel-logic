using Xunit;
using Game.Components;

namespace pixel_logic.Tests;

public class ComponentListTest
{
    [Fact]
    public void AddComponent()
    {
        ComponentList list = new ComponentList();
        int cnt = list.Count;
        Field g = new Field(1,1);
        g.name = "test";
        list.custom.Add(15, g);
        Assert.Equal(cnt+1, list.Count);
        Assert.Equal("test", list.getName(15));
    }
}