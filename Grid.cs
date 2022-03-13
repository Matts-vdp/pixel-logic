using Raylib_cs;
using System.Numerics;
using System.Text.Json;
namespace game
{
    public struct Pos
    {
        public int x;
        public int y;
        public Pos(int X, int Y)
        {
            x = X;
            y = Y;
        }
    }

    class Grid
    {
        types[,] grid;
        int[,] labels;
        Dictionary<int, Component> components;
        List<Button> buttons;
        List<Connection> connections;
        int width, height;

        public Grid(int w, int h, int size)
        {
            grid = new types[h, w];
            labels = new int[h, w];
            components = new Dictionary<int, Component>();
            connections = new List<Connection>();
            buttons = new List<Button>();
            width = w;
            height = h;
        }
        public int toGrid(float pos, int gridsize, int off)
        {
            pos = (pos + off) / gridsize;
            return (int)pos;
        }
        public void add(Vector2 pos, types t, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            add(x, y, t);
            buildObjects();
        }
        public void addNoUpdate(Vector2 pos, types t, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            add(x, y, t);
        }
        public void add(int x, int y, types t)
        {
            if (x < 0 || x >= width) { return; }
            if (y < 0 || y >= height) { return; }
            if (grid[y, x] == t) { return; }
            grid[y, x] = t;
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
        public types GetBlock(int x, int y)
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
                    Connection c = ComponentFactory.NewConnection(grid[y, x], new Pos(x, y));
                    connections.Add(c);
                    if (GetBlock(x - 1, y) == types.WIRE && GetBlock(x + 1, y) == types.WIRE)
                    {
                        changeLabel(labels[y, x + 1], labels[y, x - 1], labels);
                    }
                    if (GetBlock(x, y - 1) == types.WIRE && GetBlock(x, y + 1) == types.WIRE)
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
                        Component c = ComponentFactory.NewComponent(grid[y, x]);
                        c.add(new Pos(x, y));
                        if (grid[y, x] == types.BUT)
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
                    Connection con = ComponentFactory.NewConnection(grid[y, x], new Pos(x, y));
                    foreach (Pos pos in neighbors)
                    {
                        types block = GetBlock(pos.x, pos.y);
                        if (block == 0) { continue; }
                        if (!wiref && block == types.WIRE)
                        {
                            con.addWire(components[labels[pos.y, pos.x]]);
                            wiref = true;
                        }
                        else if (!otherf && labels[pos.y, pos.x] >= 0 && block != types.WIRE)
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
                        if (labels[y, x] == 0 && (grid[y, x] == types.OUT || grid[y, x] == types.IN))
                        {
                            labels[y, x] = -1;
                            changed = true;
                            continue;
                        }
                        if (labels[y, x] == 0 && grid[y, x] == types.CROSS)
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
            foreach (Connection c in connections)
            {
                c.updateIn();
            }
            foreach (Connection c in connections)
            {
                c.updateOut();
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

        public string toText()
        {
            string str = "";
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    str += ((int)grid[y, x]).ToString() + ' ';
                }
                str += ';';
            }
            return str;
        }
        public void fromText(string text)
        {
            int y = 0; int x = 0;
            string num = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i].Equals(' '))
                {
                    grid[y, x] = (types)Int32.Parse(num);
                    num = "";
                    x++;
                    continue;
                }
                if (text[i].Equals(';'))
                {
                    y++;
                    x = 0;
                    continue;
                }
                num += text[i];
            }
            buildObjects();
        }

        public Grid copy(int xstart, int ystart, int xend, int yend, int gridsize)
        {
            Grid newGrid = new Grid(xend - xstart + 1, yend - ystart + 1, gridsize);
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
        public void merge(Grid other, Vector2 pos, int gridsize, int xoff, int yoff)
        {
            for (int y = 0; y < other.height; y++)
            {
                for (int x = 0; x < other.width; x++)
                {
                    if (other.grid[y, x] != types.NONE)
                    {
                        addNoUpdate(pos, other.grid[y, x], gridsize, xoff + x * gridsize, yoff + y * gridsize);
                    }
                }
            }
            buildObjects();
        }
    }


}