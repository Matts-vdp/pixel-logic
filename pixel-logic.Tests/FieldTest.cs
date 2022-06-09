using System.Numerics;

using Game.Components;

using Xunit;

namespace Pixel_logic.Tests;

public class FieldTest
{
    [Theory]
    [InlineData(300, 30, 0, 10)]
    [InlineData(0, 20, 0, 0)]
    [InlineData(0, 10, 100, 10)]
    public void ToGrid(float pos, int size, int offset, int result)
    {
        int gridPos = Field.ToGrid(pos, size, offset);
        Assert.True(gridPos == result);
    }

    [Theory]
    [InlineData(0, 0, (int)Types.OUT, false)]
    [InlineData(0, 1, (int)Types.OUT, true)]
    [InlineData(0, 0, (int)Types.IN, true)]
    [InlineData(0, 100, (int)Types.OUT, false)]
    public void Add(int x, int y, int t, bool result)
    {
        Field g = new(5, 5, new File());
        bool change = g.Add(new Vector2(0, 0), (int)Types.OUT, 1, 0, 0);
        Assert.True(change);
        change = g.Add(new Vector2(x, y), t, 1, 0, 0);
        Assert.Equal(change, result);
    }
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(0, 1, false)]
    [InlineData(0, 100, false)]
    public void Del(int x, int y, bool result)
    {
        Field g = new(5, 5, new File());
        bool change = g.Add(new Vector2(0, 0), (int)Types.OUT, 1, 0, 0);
        Assert.True(change);
        change = g.Del(new Vector2(x, y), 1, 0, 0);
        Assert.Equal(change, result);
    }


    [Fact]
    public void BuildComponents()
    {
        Field g = new(10, 10, new File());
        g.Add(new Vector2(0, 0), (int)Types.OUT, 1, 0, 0);
        g.Add(new Vector2(0, 1), (int)Types.WIRE, 1, 0, 0);
        g.Add(new Vector2(0, 2), (int)Types.IN, 1, 0, 0);
        g.Add(new Vector2(0, 3), (int)Types.FF, 1, 0, 0);
        g.Add(new Vector2(1, 3), (int)Types.FF, 1, 0, 0);
        g.Add(new Vector2(0, 4), (int)Types.FF, 1, 0, 0);
        g.Add(new Vector2(1, 4), (int)Types.FF, 1, 0, 0);
        g.Add(new Vector2(2, 3), (int)Types.CLKIN, 1, 0, 0);
        g.Add(new Vector2(1, 5), (int)Types.OUT, 1, 0, 0);
        g.Add(new Vector2(1, 6), (int)Types.WIRE, 1, 0, 0);
        g.Add(new Vector2(1, 7), (int)Types.CROSS, 1, 0, 0);
        g.Add(new Vector2(0, 7), (int)Types.WIRE, 1, 0, 0);
        g.Add(new Vector2(2, 7), (int)Types.WIRE, 1, 0, 0);
        g.Add(new Vector2(1, 8), (int)Types.WIRE, 1, 0, 0);
        g.Add(new Vector2(1, 9), (int)Types.IN, 1, 0, 0);
        Component comp = g.CreateComponent(g.State);
        State state = new(5, 5);
        comp.Add(new Pos(0, 0));
        Connection output = new OutConnection(new Pos(0, 1), state);
        Connection input = new InConnection(new Pos(0, 2), state);
        Component wire = WireComp.NewComponent(state);
        Connection clk = new ClockIn(new Pos(0, 3), state);
        input.AddOther(comp);
        output.AddOther(comp);
        clk.AddOther(comp);
        clk.AddWire(wire);
        comp.Update();
        Assert.False(output.IsActive()[0]);
        input.SetActive(Value.True());
        comp.Update();
        Assert.False(output.IsActive()[0]);
        clk.SetActive(Value.True());
        comp.Update();
        comp.Update();
        Assert.True(output.IsActive()[0]);
    }

    [Fact]
    public void Load()
    {
        string json = "{\"Width\":1,\"Height\":1,\"Blocks\":[{\"P\":{\"X\":0,\"Y\":0},\"Block\":1}]}";
        string name = "test";
        Grid g = new(1, 1);
        ComponentList list = new(new File());
        g.Set(0, 0, 10);
        Field f = new(g, list);

        f.Load(name, json, new File());
        Assert.Equal(1, g[0, 0]);
    }

    [Fact]
    public void Save()
    {
        File file = new();
        int[,] grid = new int[1, 1] { { 1 } };
        Grid g = new(1, 1, grid);
        ComponentList list = new(file);
        Field f = new(g, list);
        f.Save("test", file);
        string truth = "{\"Width\":1,\"Height\":1,\"Blocks\":[{\"P\":{\"X\":0,\"Y\":0},\"Block\":1}],\"Components\":{}}";
        Assert.Equal(truth, file.Result);
    }

    [Fact]
    public void Paste()
    {
        string name = "test.cpl";
        File file = new("return 1;");
        Grid g = new(5, 5);
        ComponentList list = new(file);
        int block = 5;
        g.Set(0, 0, block);
        Field f = new(g, list);
        int cnt = list.Count;

        ComponentList list2 = new(file);
        int index = list.Add(name);
        Grid g2 = new(3, 3);
        g2.Set(1, 1, index);
        Field newF = new(g2, list2);
        f.Paste(newF, new Vector2(0, 0), 1, 0, 0);

        Assert.Equal(index, g[1, 1]);
        Assert.Equal(cnt + 1, list.Count);
    }

    [Fact]
    public void CopyPaste()
    {
        string name = "test.cpl";
        File file = new("return 1;");
        Grid g = new(5, 5);
        ComponentList list = new(file);
        int index = list.Add(name);
        int cnt = list.Count;
        g.Set(0, 0, index);
        Field f = new(g, list);

        Field newF = f.Copy(0, 0, 1, 1);
        newF.Add(new Vector2(0, 1), index, 1, 0, 0);
        f.Paste(newF, new Vector2(0, 0), 1, 0, 0);

        Assert.Equal(index, g[0, 1]);
        Assert.Equal(cnt, list.Count);
    }

    [Fact]
    public void CutPaste()
    {
        int block = 5;
        File file = new();
        Grid g = new(5, 5);
        ComponentList list = new(file);
        int cnt = list.Count;
        g.Set(0, 0, block);
        Field f = new(g, list);

        Field newF = f.Cut(0, 0, 1, 1);
        newF.Add(new Vector2(0, 0), block, 1, 0, 0);
        f.Paste(newF, new Vector2(0, 1), 1, 0, 0);

        Assert.Equal(block, g[0, 1]);
        Assert.Equal(0, g[0, 0]);
        Assert.Equal(cnt, list.Count);
    }
}