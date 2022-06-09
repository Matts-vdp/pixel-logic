using Game.Components;

using Xunit;

namespace Pixel_logic.Tests;

public class ComponentListTest
{
    [Fact]
    public void Count()
    {
        ComponentList list = new(new File());
        int cnt = list.Count;
        Field g = new(1, 1, new File())
        {
            Name = "test"
        };
        list.Custom.Add(17, g);
        Assert.Equal(cnt + 1, list.Count);
        Assert.Equal("test", list.GetName(17));
    }

    [Fact]
    public void AddField()
    {
        string name = "test.json";
        string json = "{\"width\":1,\"height\":1,\"blocks\":[{\"pos\":{\"x\":0,\"y\":0},\"block\":1}]}";
        IFile file = new File(json);
        ComponentList list = new(file);
        int cnt = list.Count;
        list.Add(name);
        Assert.Equal(cnt + 1, list.Count);
        Assert.Equal(name, list.Custom[cnt + 1].Name);
    }

    [Fact]
    public void AddCode()
    {
        string name = "test.cpl";
        string content = "return i;";
        IFile file = new File(content);
        ComponentList list = new(file);
        int cnt = list.Count;
        list.Add(name);
        Assert.Equal(cnt + 1, list.Count);
        Assert.Equal(name, list.Custom[cnt + 1].Name);
    }

    [Fact]
    public void AddSame()
    {
        string name = "test.cpl";
        string content = "return i;";
        IFile file = new File(content);
        ComponentList list = new(file);
        int cnt = list.Count;
        list.Add(name);
        Assert.Equal(cnt + 1, list.Count);
        Assert.Equal(name, list.Custom[cnt + 1].Name);
        int index = list.Add(name);
        Assert.Equal(cnt + 1, list.Count);
        Assert.Equal(name, list.Custom[cnt + 1].Name);
        Assert.Equal(cnt + 1, index);
    }
}