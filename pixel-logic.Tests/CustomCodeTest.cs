using Xunit;
using Game.Components;
using System.Collections.Generic;
namespace pixel_logic.Tests;

public class CustomCodeTest
{
    [Fact]
    public void InputGetSet()
    {
        Input input = new Input();
        input.i.Add(true);
        Assert.True(input.get(0));
        Assert.False(input.get(1));
        input.set(0,false);
        Assert.False(input.o[0]);
        input.set(1,true);
        Assert.True(input.o[1]);
    }
    [Fact]
    public void InputInt()
    {
        Input input = new Input();
        uint i = 5;
        List<bool> list = input.fromInt(i);
        Assert.Equal(new List<bool>{true,false,true}, list);
        uint result = input.toInt(list);
        Assert.Equal(i, result);
    }
    [Fact]
    public void InputArray()
    {
        Input input = new Input();
        List<bool> list = new List<bool>{true,false,true};
        bool[] array = input.toArray(list);
        Assert.Equal(new bool[]{true,false,true}, array);
        list = input.fromArray(array);
        Assert.Equal(new List<bool>{true,false,true}, list);
    }
    [Fact]
    public void InputStore()
    {
        Input input = new Input();
        uint i = 5;
        input.save("test", i);
        uint result = (uint)input.load("test");
        Assert.Equal(i, result);
    }

    [Fact]
    public void CCode()
    {
        string code = "i[0] = false; return i;";
        CCode ccode = new CCode("test.cpl", code);
        Input input = new Input();
        input.i = new List<bool>{true,false};
        List<bool> result = ccode.run(input);
        List<bool> truth = new List<bool>{false,false};
        Assert.Equal(truth, result);
    }
    [Fact]
    public void Pprogram()
    {
        string code = "return i;";
        CCode ccode = new CCode("test.ppl", code);
        State state = new State(5,5);
        Component comp = ccode.createComponent(state);
        Connection input = new InConnection(new Pos(0,1), state);
        Connection output = new OutConnection(new Pos(0,2), state);
        Connection clock = new ClockIn(new Pos(0,3), state);
        input.addOther(comp);
        output.addOther(comp);
        clock.addOther(comp);

        comp.update();
        Assert.False(output.isActive());
        input.setActive(true);
        comp.update();
        Assert.False(output.isActive());
        clock.setActive(true);
        comp.update();
        Assert.True(output.isActive());
    }

    [Fact]
    public void Cprogram()
    {
        string code = "return i;";
        CCode ccode = new CCode("test.cpl", code);
        State state = new State(5,5);
        Component comp = ccode.createComponent(state);
        Connection input = new InConnection(new Pos(0,1), state);
        Connection output = new OutConnection(new Pos(0,2), state);
        input.addOther(comp);
        output.addOther(comp);

        comp.update();
        Assert.False(output.isActive());
        input.setActive(true);
        comp.update();
        Assert.True(output.isActive());
        comp.update();
        Assert.True(output.isActive());
    }
}