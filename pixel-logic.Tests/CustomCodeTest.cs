using System.Collections.Generic;

using Game.Components;

using Xunit;
namespace Pixel_logic.Tests;

public class CustomCodeTest
{
    [Fact]
    public void InputGetSet()
    {
        Input input = new();
        input.I[0].Values.Add(true);
        Assert.True(input.Get(0)[0]);
        Assert.False(input.Get(1)[0]);
        input.Set(0, false);
        Assert.False(input.O[0][0]);
        input.Set(1, true);
        Assert.True(input.O[1][0]);
    }
    [Fact]
    public void InputInt()
    {
        Input input = new();
        uint i = 5;
        List<bool> list = input.FromInt(i);
        Assert.Equal(new List<bool> { true, false, true }, list);
        uint result = input.ToInt(list);
        Assert.Equal(i, result);
    }
    [Fact]
    public void InputArray()
    {
        Input input = new();
        List<bool> list = new() { true, false, true };
        bool[] array = input.ToArray(list);
        Assert.Equal(new bool[] { true, false, true }, array);
        list = input.FromArray(array);
        Assert.Equal(new List<bool> { true, false, true }, list);
    }
    [Fact]
    public void InputStore()
    {
        Input input = new();
        uint i = 5;
        input.Save("test", i);
        uint result = (uint)input.Load("test");
        Assert.Equal(i, result);
    }

    [Fact]
    public void CCode()
    {
        string code = "Set(0, true); return O;";
        CCode ccode = new("test.cpl", code);
        Input input = new();
        List<bool> result = ccode.Run(input)[0].Values;
        List<bool> truth = new() { true, false };
        Assert.Equal(truth, result);
    }
    [Fact]
    public void Pprogram()
    {
        string code = "return I;";
        CCode ccode = new("test.ppl", code);
        State state = new(5, 5);
        Component comp = ccode.CreateComponent(state);
        Connection input = new InConnection(new Pos(0, 1), state);
        Connection output = new OutConnection(new Pos(0, 2), state);
        Connection clock = new ClockIn(new Pos(0, 3), state);
        input.AddOther(comp);
        output.AddOther(comp);
        clock.AddOther(comp);

        comp.Update();
        Assert.False(output.IsActive()[0]);
        input.SetActive(Value.True());
        comp.Update();
        Assert.False(output.IsActive()[0]);
        clock.SetActive(Value.True());
        comp.Update();
        Assert.True(output.IsActive()[0]);
    }

    [Fact]
    public void Cprogram()
    {
        string code = "return I;";
        CCode ccode = new("test.cpl", code);
        State state = new(5, 5);
        Component comp = ccode.CreateComponent(state);
        Connection input = new InConnection(new Pos(0, 1), state);
        Connection output = new OutConnection(new Pos(0, 2), state);
        input.AddOther(comp);
        output.AddOther(comp);

        comp.Update();
        Assert.False(output.IsActive()[0]);
        input.SetActive(new Value());
        comp.Update();
        Assert.True(output.IsActive()[0]);
        comp.Update();
        Assert.True(output.IsActive()[0]);
    }
}