using System.Collections.Generic;
using System.Numerics;

using Game.Components;

using Xunit;

namespace Pixel_logic.Tests;

public class SaveTest
{
    [Fact]
    public void Save()
    {
        int[,] grid = new int[1, 1]{
            {1},
        };
        Dictionary<int, ComponentCreator> custom = new();
        Field f = new(1, 1, new File())
        {
            Name = "test"
        };
        custom.Add(1, f);
        SaveData save = new(1, 1, grid, custom);

        Assert.Equal(new BlockPos(0, 0, 1), save.Blocks[0]);
        Assert.Equal("test", save.Components[1]);

        string json = save.ToJson();
        string truth = "{\"Width\":1,\"Height\":1,\"Blocks\":[{\"P\":{\"X\":0,\"Y\":0},\"Block\":1}],\"Components\":{\"1\":\"test\"}}";
        Assert.Equal(truth, json);
    }

    [Fact]
    public void Load()
    {
        string name = "test.cpl";
        string js = "{\"Width\":1,\"Height\":1,\"Blocks\":[{\"P\":{\"X\":0,\"Y\":0},\"Block\":1}],\"Components\":{\"1\":\"" + name + "\"}}";
        SaveData save = SaveData.FromJson(js);
        Assert.Equal(new BlockPos(0, 0, 1), save.Blocks[0]);
        Assert.Equal(name, save.Components[1]);

        int[,] grid = save.FromArray();
        int[,] truth = new int[1, 1] { { 1 } };
        Assert.Equal(truth, grid);

        IFile file = new File();
        ComponentList list = save.ReadComponents(grid, file);
        Assert.Equal(name, list.Custom[list.Count].Name);
    }
}