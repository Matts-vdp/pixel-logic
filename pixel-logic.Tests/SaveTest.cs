using Xunit;
using Game.Components;
using System.Numerics;
using System.Collections.Generic;

namespace pixel_logic.Tests;

public class SaveTest
{
    [Fact]
    public void Save()
    {
        int[,] grid = new int[1,1]{
            {1},
        };
        Dictionary<int,ComponentCreator> custom = new Dictionary<int, ComponentCreator>();
        Field f = new Field(1,1, new File());
        f.name = "test";
        custom.Add(1, f);
        SaveData save = new SaveData(1,1, grid, custom);
        
        Assert.Equal(new BlockPos(0,0,1), save.blocks[0]);
        Assert.Equal("test", save.components[1]);

        string json = save.toJson();
        string truth = "{\"width\":1,\"height\":1,\"blocks\":[{\"pos\":{\"x\":0,\"y\":0},\"block\":1}],\"components\":{\"1\":\"test\"}}";
        Assert.Equal(truth, json);
    }

    [Fact]
    public void Load()
    {
        string name = "test.cpl";
        string js = "{\"width\":1,\"height\":1,\"blocks\":[{\"pos\":{\"x\":0,\"y\":0},\"block\":1}],\"components\":{\"1\":\""+name+"\"}}";
        SaveData save = SaveData.fromJson(js);
        Assert.Equal(new BlockPos(0,0,1), save.blocks[0]);
        Assert.Equal(name, save.components[1]);

        int[,] grid = save.fromArray();
        int[,] truth = new int[1,1]{{1}};
        Assert.Equal(truth, grid);

        IFile file = new File();
        ComponentList list = save.readComponents(grid, file);
        Assert.Equal(name, list.custom[list.Count].name);
    }
}