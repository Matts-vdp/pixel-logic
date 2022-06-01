using Xunit;
using Game.Components;

namespace pixel_logic.Tests;

public class ComponentListTest
{
    [Fact]
    public void Count()
    {
        ComponentList list = new ComponentList(new File());
        int cnt = list.Count;
        Field g = new Field(1,1, new File());
        g.name = "test";
        list.custom.Add(15, g);
        Assert.Equal(cnt+1, list.Count);
        Assert.Equal("test", list.getName(15));
    }

    [Fact]
    public void AddField()
    {
        string name = "test.json";
        string json = "{\"width\":1,\"height\":1,\"blocks\":[{\"pos\":{\"x\":0,\"y\":0},\"block\":1}]}";
        IFile file = new File(json);
        ComponentList list = new ComponentList(file);
        int cnt = list.Count;
        list.add(name);
        Assert.Equal(cnt+1, list.Count);
        Assert.Equal(name, list.custom[cnt+1].name);
    }

    [Fact]
    public void AddCode()
    {
        string name = "test.cpl";
        string content = "return i;";
        IFile file = new File(content);
        ComponentList list = new ComponentList(file);
        int cnt = list.Count;
        list.add(name);
        Assert.Equal(cnt+1, list.Count);
        Assert.Equal(name, list.custom[cnt+1].name);
    }

    [Fact]
    public void AddSame()
    {
        string name = "test.cpl";
        string content = "return i;";
        IFile file = new File(content);
        ComponentList list = new ComponentList(file);
        int cnt = list.Count;
        list.add(name);
        Assert.Equal(cnt+1, list.Count);
        Assert.Equal(name, list.custom[cnt+1].name);
        int index = list.add(name);
        Assert.Equal(cnt+1, list.Count);
        Assert.Equal(name, list.custom[cnt+1].name);
        Assert.Equal(cnt+1, index);
    }
}