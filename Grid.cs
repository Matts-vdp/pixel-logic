using Raylib_cs;
using System.Numerics;
using System.Text.Json;
namespace game
{
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
    class Grid : CustomComponent
    {
        int[,] grid;
        int[,] labels;
        Dictionary<int, Component> components;

        List<Button> buttons;
        List<Connection> connections;
        int width, height;

        public ComponentList list;
        public Grid(int w, int h) : this(w, h, new ComponentList()) { }

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

        public Grid(string text)
        {
            name = text;
            SaveData save = SaveData.fromJson(text);
            grid = fromArray(save.width, save.height, save.blocks);
            list = ComponentList.fromDict(save.components);
            width = save.width;
            height = save.height;

            labels = new int[height, width];
            components = new Dictionary<int, Component>();
            connections = new List<Connection>();
            buttons = new List<Button>();
            buildObjects();
        }

        public List<BlockPos> toArray()
        {
            List<BlockPos> pos = new List<BlockPos>();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (grid[y, x] != 0)
                        pos.Add(new BlockPos(x, y, grid[y, x]));
            return pos;
        }
        public SaveData toSave()
        {
            return new SaveData(width, height, toArray(), list.toSave());
        }

        public int[,] fromArray(int width, int height, List<BlockPos> pos)
        {
            int[,] world = new int[height, width];
            foreach (BlockPos p in pos)
            {
                world[p.pos.y, p.pos.x] = p.block;
            }
            return world;
        }
        public int toGrid(float pos, int gridsize, int off)
        {
            pos = (pos + off) / gridsize;
            return (int)pos;
        }
        public void add(Vector2 pos, int t, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            bool changed = add(x, y, t);
            if (changed)
                buildObjects();
        }
        public void addNoUpdate(Vector2 pos, int t, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            add(x, y, t);
        }
        public bool add(int x, int y, int t)
        {
            if (x < 0 || x >= width) { return false; }
            if (y < 0 || y >= height) { return false; }
            if (grid[y, x] == t) { return false; }
            grid[y, x] = t;
            return true;
        }
        public void del(Vector2 pos, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            if (x < 0 || x >= width) { return; }
            if (y < 0 || y >= height) { return; }
            grid[y, x] = 0;
            buildObjects();
        }
        public int GetBlock(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return 0;
            }
            return grid[y, x];
        }
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
        public void printlabels(int[,] labels)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.Write(labels[y, x].ToString() + ", ");
                }
                Console.Write("\n");
            }
        }
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
        public string toJson()
        {
            Console.WriteLine(toSave().toJson());
            return toSave().toJson();
        }

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
            newGrid.buildObjects();
            return newGrid;
        }

        public Grid cut(int xstart, int ystart, int xend, int yend)
        {
            Grid newGrid = new Grid(xend - xstart + 1, yend - ystart + 1, list);
            for (int y = ystart; y < yend + 1; y++)
            {
                for (int x = xstart; x < xend + 1; x++)
                {
                    newGrid.grid[y - ystart, x - xstart] = grid[y, x];
                    grid[y, x] = (int)types.NONE;
                }
            }
            buildObjects();
            newGrid.buildObjects();
            return newGrid;
        }

        public void merge(Grid other, Vector2 pos, int gridsize, int xoff, int yoff)
        {
            // add custom components
            foreach (int key in other.list.components.Keys)
            {
                string name = Path.GetFileNameWithoutExtension(other.list.components[key].name);
                int index = list.items.IndexOf(name);
                if (index != -1)
                {
                    other.grid = other.changeLabel(key, index+1, other.grid);
                }
                else
                {
                    list.add(other.list.components[key].name);
                    int i = list.items.Count;
                    other.grid = other.changeLabel(key, i, other.grid);
                }
            }
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
        public void mergeZero(Grid other)
        {
            for (int y = 0; y < other.height; y++)
            {
                for (int x = 0; x < other.width; x++)
                {
                    if (other.grid[y, x] != (int)types.NONE)
                    {
                        add(x, y, other.grid[y, x]);
                    }
                }
            }
            buildObjects();
        }

        public override Component toComponent(ComponentList list, int type = 0)
        {
            buildObjects();
            List<Connection> inp = new List<Connection>();
            List<Connection> outp = new List<Connection>();
            Connection? clock = null;

            foreach (Connection i in connections)
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
            // TODO: hier benk gesropt
            return new SubComponent(this, inp, outp, clock, list);
        }

        public void clear()
        {
            grid = new int[height, width];
        }


        public void save(string filename)
        {
            File.WriteAllTextAsync("saves/" + filename, toJson());
        }

        public void load()
        {
            string txt = File.ReadAllText("saves/save.dpl");
            clear();
            mergeZero(new Grid(txt));
        }
    }


}