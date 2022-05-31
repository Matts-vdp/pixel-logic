using System.Text.Json;

namespace Game.Components
{
    // couples a coordinate to a block type
    // used for saving a grid to json
    public struct BlockPos
    {
        public Pos pos { get; set; }
        public int block { get; set; }
        public BlockPos(int x, int y, int b)
        {
            pos = new Pos(x, y);
            block = b;
        }
    }

    // used to store a grid and its componentlist to json
    public class SaveData
    {
        public int width { get; set; }
        public int height { get; set; }
        public List<BlockPos> blocks { get; set; }
        public Dictionary<int, string> components { get; set; }
        public SaveData(int w, int h, List<BlockPos> grid, Dictionary<int, string> components)
        {
            this.blocks = grid;
            this.components = components;
            width = w;
            height = h;
        }
        public SaveData() : this(200, 200, new List<BlockPos>(), new Dictionary<int, string>()) { }
        public SaveData(int width, int height, int[,] grid, Dictionary<int, ComponentCreator> custom)
        {
            this.width = width;
            this.height = height;
            blocks = toArray(grid);
            Dictionary<int, bool> block = new Dictionary<int, bool>();
            foreach (BlockPos bp in blocks)
            {
                block[bp.block] = true;
            }
            components = toSave(block, custom);
        }

        //SAVE
        public Dictionary<int, string> toSave(Dictionary<int, bool> blocks, Dictionary<int, ComponentCreator> custom)
        {
            Dictionary<int, string> names = new Dictionary<int, string>();
            foreach (int key in custom.Keys)
            {
                if (blocks.ContainsKey(key))
                    names[key] = custom[key].name;
            }
            return names;
        }

        // converts grid into a list of BlockPos because json cant serialize int[,] 
        private List<BlockPos> toArray(int[,] grid)
        {
            List<BlockPos> pos = new List<BlockPos>();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (grid[y, x] != 0)
                        pos.Add(new BlockPos(x, y, grid[y, x]));
            return pos;
        }
        
        public string toJson()
        {
            return JsonSerializer.Serialize<SaveData>(this);
        }

        
        //LOAD
        // converts list of BlockPos into grid because json cant serialize int[,]
        public int[,] fromArray()
        {
            int[,] world = new int[height, width];
            foreach (BlockPos p in blocks)
            {
                world[p.pos.y, p.pos.x] = p.block;
            }
            return world;
        }
        // loads the custom components from save into ComponentList
        public ComponentList readComponents(int[,] grid)
        {
            ComponentList cList = new ComponentList();
            foreach (int key in components.Keys)
            {
                int newIndex = cList.add(components[key]);
                grid = Grid.changeLabel(key, newIndex, grid, width, height);
            }
            return cList;
        }
    
        public static SaveData fromJson(string text)
        {
            SaveData? save = JsonSerializer.Deserialize<SaveData>(text);
            if (save != null) return save;
            return new SaveData();
        }
    }
}