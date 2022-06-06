using System.Text.Json;

namespace Game.Components
{
    // couples a coordinate to a block type
    // used for saving a grid to json
    public struct BlockPos
    {
        public Pos P { get; set; }
        public int Block { get; set; }
        public BlockPos(int x, int y, int b)
        {
            P = new Pos(x, y);
            Block = b;
        }
    }

    // used to store a grid and its componentlist to json
    public class SaveData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<BlockPos> Blocks { get; set; }
        public Dictionary<int, string> Components { get; set; }

        public SaveData(int width, int height, int[,] grid, Dictionary<int, ComponentCreator> custom)
        {
            Width = width;
            Height = height;
            Blocks = ToArray(grid);
            Dictionary<int, bool> block = new();
            foreach (BlockPos bp in Blocks)
            {
                block[bp.Block] = true;
            }
            Components = ToSave(block, custom);
        }
        public SaveData() : this(1, 1, new int[1, 1], new Dictionary<int, ComponentCreator>()) { }


        //SAVE
        private static Dictionary<int, string> ToSave(Dictionary<int, bool> blocks, Dictionary<int, ComponentCreator> custom)
        {
            Dictionary<int, string> names = new();
            foreach (int key in custom.Keys)
            {
                if (blocks.ContainsKey(key))
                    names[key] = custom[key].Name;
            }
            return names;
        }

        // converts grid into a list of BlockPos because json cant serialize int[,] 
        private List<BlockPos> ToArray(int[,] grid)
        {
            List<BlockPos> pos = new();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (grid[x, y] != 0)
                        pos.Add(new BlockPos(x, y, grid[x, y]));
            return pos;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize<SaveData>(this);
        }


        //LOAD
        // converts list of BlockPos into grid because json cant serialize int[,]
        public int[,] FromArray()
        {
            int[,] world = new int[Height, Width];
            foreach (BlockPos p in Blocks)
            {
                world[p.P.X, p.P.Y] = p.Block;
            }
            return world;
        }
        // loads the custom components from save into ComponentList
        public ComponentList ReadComponents(int[,] grid, IFile file)
        {
            ComponentList cList = new(file);
            foreach (int key in Components.Keys)
            {
                int newIndex = cList.Add(Components[key]);
                grid = Grid.ChangeLabel(key, newIndex, grid, Width, Height);
            }
            return cList;
        }

        public static SaveData FromJson(string text)
        {
            SaveData? save = JsonSerializer.Deserialize<SaveData>(text);
            if (save != null) return save;
            return new SaveData();
        }
    }
}