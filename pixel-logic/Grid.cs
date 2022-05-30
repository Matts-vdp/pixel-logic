using Raylib_cs;
using System.Numerics;
using System.Text.Json;
namespace game
{
    // struct to contain a 2d coordinate
    public struct Pos
    {
        public int x { get; set; }
        public int y { get; set; }
        public Pos(int X, int Y)
        {
            x = X;
            y = Y;
        }
    }

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
    class SaveData
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
        public static SaveData fromJson(string text)
        {
            SaveData? save = JsonSerializer.Deserialize<SaveData>(text);
            if (save != null) return save;
            return new SaveData();
        }
        public string toJson()
        {
            return JsonSerializer.Serialize<SaveData>(this);
        }
    }

    // represents a grid of components converts the grid matrix to components using connected components
    class Grid : CustomComponentCreator
    {
        int width, height;                      // size of grid
        int[,] grid;                            // contains wich block is placed where
        int[,] labels;                          // used for connected component analysis
        Dictionary<int, Component> components;  // stores all components
        List<Button> buttons;                   // stores buttons for input handling
        List<Connection> connections;           // stores all connections between components
        public ComponentList list;              // stores the mapping of block types to components

        // used to create new grid
        public Grid(int w, int h) : this(w, h, new ComponentList()) { }
        // used to create new grid with same componentList 
        public Grid(int w, int h, ComponentList list)
        {
            this.list = list;
            grid = new int[h, w];
            labels = new int[h, w];
            components = new Dictionary<int, Component>();
            connections = new List<Connection>();
            buttons = new List<Button>();
            width = w;
            height = h;
        }
        //------------------------------------------------------------------

        // JSON
        // used to load a grid from a json file
        public Grid(string text)
        {
            // load json
            name = Path.GetFileName(text);
            string js = File.ReadAllText("saves/circuit/" + name);
            SaveData save = SaveData.fromJson(js);
            width = save.width;
            height = save.height;
            grid = fromArray(save.width, save.height, save.blocks);
            list = RebaseComponents(save.components);
            // init other objects
            labels = new int[height, width];
            components = new Dictionary<int, Component>();
            connections = new List<Connection>();
            buttons = new List<Button>();
            buildObjects();
        }
        // saves grid to saves/circuit/filename
        public void save(string filename)
        {
            File.WriteAllTextAsync("saves/circuit/" + filename, toSave().toJson());
        }
        // loads grid from saves/circuit/save.json
        public void load(int gridsize)
        {
            clear();
            paste(new Grid("saves/circuit/save.json"), gridsize);
        }
        // converts grid into a list of BlockPos because json cant serialize int[,] 
        public List<BlockPos> toArray()
        {
            List<BlockPos> pos = new List<BlockPos>();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (grid[y, x] != 0)
                        pos.Add(new BlockPos(x, y, grid[y, x]));
            return pos;
        }
        // creates a saveData object from this grid object
        public SaveData toSave()
        {
            List<BlockPos> blockPositions = toArray();
            Dictionary<int, bool> blocks = new Dictionary<int, bool>();
            foreach (BlockPos bp in blockPositions)
            {
                blocks[bp.block] = true;
            }
            return new SaveData(width, height, toArray(), list.toSave(blocks));
        }
        // converts list of BlockPos into grid because json cant serialize int[,]
        public int[,] fromArray(int width, int height, List<BlockPos> pos)
        {
            int[,] world = new int[height, width];
            foreach (BlockPos p in pos)
            {
                world[p.pos.y, p.pos.x] = p.block;
            }
            return world;
        }
        // converts world coordinates to grid coordinates
        public int toGrid(float pos, int gridsize, int off)
        {
            pos = (pos + off) / gridsize;
            return (int)pos;
        }
        // loads the custom components from save into ComponentList
        public ComponentList RebaseComponents(Dictionary<int, string> comp)
        {
            ComponentList cList = new ComponentList();
            foreach (int key in comp.Keys)
            {
                int newIndex = cList.add(comp[key]);
                grid = changeLabel(key, newIndex, grid);
            }
            return cList;
        }
        //------------------------------------------------------------------

        // CHANGE GRID
        // add block to grid with world coordinates
        public void add(Vector2 pos, int t, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            bool changed = add(x, y, t);
            if (changed)
                buildObjects();
        }
        // add without rebuilding the grid components
        public void addNoUpdate(Vector2 pos, int t, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            add(x, y, t);
        }
        // add block to grid with grid coordinates
        public bool add(int x, int y, int t)
        {
            if (x < 0 || x >= width) { return false; }
            if (y < 0 || y >= height) { return false; }
            if (grid[y, x] == t) { return false; }
            grid[y, x] = t;
            return true;
        }
        // remove block from grid
        public void del(Vector2 pos, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            if (x < 0 || x >= width) { return; }
            if (y < 0 || y >= height) { return; }
            grid[y, x] = 0;
            buildObjects();
        }
        // retreives block from grid returns 0 when outside
        public int GetBlock(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return 0;
            }
            return grid[y, x];
        }
        // changes all occurences of "from" to "to" in the grid
        public int[,] changeLabel(int from, int to, int[,] labels)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (labels[y, x] == from)
                    {
                        labels[y, x] = to;
                    }
                }
            }
            return labels;
        }
        // clear grid
        public void clear()
        {
            grid = new int[height, width];
        }
        //------------------------------------------------------------------

        // CONVERT TO COMPONENTS
        // create components from grid matrix
        public void buildObjects()
        {
            labels = new int[height, width];
            components = new Dictionary<int, Component>();
            connections = new List<Connection>();
            buttons = new List<Button>();
            connectedComponents();
            crossConnect();
            makeComponents();
            makeConnections();
        }
        // use connected components algorithm to group blocks of the same type
        // that are connected to each other into the same component
        public void connectedComponents()
        {
            int label = 1;
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (grid[y, x] == 0) continue;
                        if (labels[y, x] == 0 && (grid[y, x] == (int)types.OUT || grid[y, x] == (int)types.IN || grid[y, x] == (int)types.CLKIN))
                        {
                            labels[y, x] = -1;
                            changed = true;
                            continue;
                        }
                        if (labels[y, x] == 0 && grid[y, x] == (int)types.CROSS)
                        {
                            labels[y, x] = -2;
                            changed = true;
                            continue;
                        }
                        bool found = false;
                        if (GetBlock(x - 1, y) == grid[y, x] && labels[y, x] != labels[y, x - 1])
                        {
                            labels[y, x] = labels[y, x - 1];
                            found = true;
                            changed = true;
                        }
                        if (GetBlock(x, y - 1) == grid[y, x] && labels[y, x] != labels[y - 1, x])
                        {
                            if (GetBlock(x - 1, y) == grid[y, x])
                            {
                                labels = changeLabel(labels[y, x], labels[y - 1, x], labels);
                            }
                            labels[y, x] = labels[y - 1, x];
                            found = true;
                            changed = true;
                        }
                        if (!found && labels[y, x] == 0)
                        {
                            labels[y, x] = label++;
                            changed = true;
                        }
                    }
                }
            }
        }
        // connect wires with a cross connection between them
        public void crossConnect()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (labels[y, x] != -2) { continue; }
                    Connection c = list.NewConnection(grid[y, x], new Pos(x, y));
                    connections.Add(c);
                    if (GetBlock(x - 1, y) == (int)types.WIRE && GetBlock(x + 1, y) == (int)types.WIRE)
                    {
                        changeLabel(labels[y, x + 1], labels[y, x - 1], labels);
                    }
                    if (GetBlock(x, y - 1) == (int)types.WIRE && GetBlock(x, y + 1) == (int)types.WIRE)
                    {
                        changeLabel(labels[y + 1, x], labels[y - 1, x], labels);
                    }
                }
            }
        }
        // create the components from the grid
        public void makeComponents()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (labels[y, x] <= 0 || grid[y, x] == 0) { continue; }
                    if (components.ContainsKey(labels[y, x]))
                    {
                        components[labels[y, x]].add(new Pos(x, y));
                    }
                    else
                    {
                        Component c = list.NewComponent(grid[y, x]);
                        c.add(new Pos(x, y));
                        if (grid[y, x] == (int)types.BUT)
                        {
                            buttons.Add((Button)c);
                        }
                        components.Add(labels[y, x], c);
                    }
                }
            }
        }
        // create the connections between components
        public void makeConnections()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (labels[y, x] != -1) { continue; }
                    bool wiref = false;
                    bool otherf = false;
                    Pos[] neighbors = { new Pos(x - 1, y), new Pos(x, y - 1), new Pos(x + 1, y), new Pos(x, y + 1) };
                    Connection con = list.NewConnection(grid[y, x], new Pos(x, y));
                    foreach (Pos pos in neighbors)
                    {
                        int block = GetBlock(pos.x, pos.y);
                        if (block == 0) { continue; }
                        if (!wiref && block == (int)types.WIRE)
                        {
                            con.addWire(components[labels[pos.y, pos.x]]);
                            wiref = true;
                        }
                        else if (!otherf && labels[pos.y, pos.x] >= 0 && block != (int)types.WIRE)
                        {
                            con.addOther(components[labels[pos.y, pos.x]]);
                            otherf = true;
                        }
                    }
                    connections.Add(con);
                }
            }
        }
        //------------------------------------------------------------------

        // GAME LOOP FUNCTIONS
        // update components
        public void update()
        {
            foreach (Component c in components.Values)
            {
                if (!(c is WireComp))
                    c.update();
            }
            foreach (Component c in components.Values)
            {
                if (c is WireComp)
                    c.update();
            }
        }
        // handle keyboard input
        public void Input()
        {
            int key = 49;
            for (int i = 0; i < buttons.Count && i < 9; i++)
            {
                if (Raylib.IsKeyPressed((KeyboardKey)(key + i)))
                {
                    buttons[i].toggle();
                }
            }

        }
        // draw all components
        public void draw(int gridsize, int xoff, int yoff)
        {
            foreach (KeyValuePair<int, Component> entry in components)
            {
                entry.Value.draw(gridsize, xoff, yoff);
            }
            foreach (Connection c in connections)
            {
                c.draw(gridsize, xoff, yoff);
            }
        }
        //------------------------------------------------------------------

        // COPY PASTE
        // copy part of grid to new grid object
        public Grid copy(int xstart, int ystart, int xend, int yend)
        {
            Grid newGrid = new Grid(xend - xstart + 1, yend - ystart + 1, list);
            for (int y = ystart; y < yend + 1; y++)
            {
                for (int x = xstart; x < xend + 1; x++)
                {
                    newGrid.grid[y - ystart, x - xstart] = grid[y, x];
                }
            }
            newGrid.name = name;
            newGrid.buildObjects();
            return newGrid;
        }
        // copy and remove in original
        public Grid cut(int xstart, int ystart, int xend, int yend)
        {
            Grid newGrid = copy(xstart, ystart, xend, yend);
            for (int y = ystart; y < yend + 1; y++)
            {
                for (int x = xstart; x < xend + 1; x++)
                {
                    grid[y, x] = (int)types.NONE;
                }
            }
            buildObjects();
            return newGrid;
        }
        // merge customcomponents of componentlist adds missing components
        // and changes references to already imported components 
        public void mergeComponents(Grid other)
        {
            foreach (int key in other.list.components.Keys)
            {
                int index = list.add(other.list.components[key].name);
                other.grid = other.changeLabel(key, index, other.grid);
            }
        }
        // paste other grid into this grid
        public void paste(Grid other, Vector2 pos, int gridsize, int xoff, int yoff)
        {
            // add custom components
            mergeComponents(other);
            for (int y = 0; y < other.height; y++)
            {
                for (int x = 0; x < other.width; x++)
                {
                    if (other.grid[y, x] != (int)types.NONE)
                    {
                        addNoUpdate(pos, other.grid[y, x], gridsize, xoff + x * gridsize, yoff + y * gridsize);
                    }
                }
            }
            buildObjects();
        }
        // paste other grid in this grid at 0, 0
        public void paste(Grid other, int gridsize)
        {
            paste(other, Vector2.Zero, gridsize, 0, 0);
        }

        //------------------------------------------------------------------
        // CUSTOM COMPONENT
        // creates a subcomponent from this grid
        public override Component toComponent(ComponentList list, int type = 0)
        {
            Grid newGrid = copy(0, 0, width - 1, height - 1);
            List<Connection> inp = new List<Connection>();
            List<Connection> outp = new List<Connection>();
            Connection? clock = null;

            foreach (Connection i in newGrid.connections)
            {
                if (!i.isFull())
                {
                    if (i.GetType() == typeof(InConnection))
                    {
                        outp.Add(i);
                    }
                    else if (i.GetType() == typeof(OutConnection))
                    {
                        inp.Add(i);
                    }
                    else if (i.GetType() == typeof(ClockIn))
                    {
                        clock = i;
                    }
                }
            }
            return new SubComponent(newGrid, inp, outp, clock);
        }

        //------------------------------------------------------------------

    }


}