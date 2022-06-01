using System.Numerics;

namespace Game.Components
{
    public class Grid
    {
        public int width;
        public int height;

        public int[,] grid;

        public Grid(int w, int h, int[,] g)
        {
            width = w;
            height = h;
            grid = g;
        }

        public Grid(int w, int h) : this(w, h, new int[w, h])
        {
        }

        public int this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                    return 0;
                return grid[x, y];
            }
        }

        // add block to grid with grid coordinates
        public bool set(int x, int y, int t)
        {
            if (x < 0 || x >= width) { return false; }
            if (y < 0 || y >= height) { return false; }
            if (grid[x, y] == t) { return false; }
            grid[x, y] = t;
            return true;
        }

        public void clear()
        {
            clear(0, 0, width, height);
        }

        public void clear(int xstart, int ystart, int xend, int yend)
        {
            for (int y = ystart; y < yend + 1; y++)
            {
                for (int x = xstart; x < xend + 1; x++)
                {
                    set(x, y, (int)types.NONE);
                }
            }
        }

        // changes all occurences of "from" to "to" in the grid
        public static int[,] changeLabel(int from, int to, int[,] labels, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (labels[x, y] == from)
                    {
                        labels[x, y] = to;
                    }
                }
            }
            return labels;
        }

        // use connected components algorithm to group blocks of the same type
        // that are connected to each other into the same component
        public int[,] connectedComponents()
        {
            int[,] labels = new int[width, height];
            int label = 1;
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (grid[x, y] == 0) continue;
                        if (labels[x, y] == 0 && (grid[x, y] == (int)types.OUT || grid[x, y] == (int)types.IN || grid[x, y] == (int)types.CLKIN))
                        {
                            labels[x, y] = -1;
                            changed = true;
                            continue;
                        }
                        if (labels[x, y] == 0 && grid[x, y] == (int)types.CROSS)
                        {
                            labels[x, y] = -2;
                            changed = true;
                            continue;
                        }
                        bool found = false;
                        if (this[x - 1, y] == grid[x, y] && labels[x, y] != labels[x - 1, y])
                        {
                            labels[x, y] = labels[x - 1, y];
                            found = true;
                            changed = true;
                        }
                        if (this[x, y - 1] == grid[x, y] && labels[x, y] != labels[x, y - 1])
                        {
                            if (this[x - 1, y] == grid[x, y])
                            {
                                labels = Grid.changeLabel(labels[x, y], labels[x, y - 1], labels, width, height);
                            }
                            labels[x, y] = labels[x, y - 1];
                            found = true;
                            changed = true;
                        }
                        if (!found && labels[x, y] == 0)
                        {
                            labels[x, y] = label++;
                            changed = true;
                        }
                    }
                }
            }
            return labels;
        }

        public Grid copy(int xstart, int ystart, int xend, int yend)
        {
            Grid newGrid = new Grid(xend - xstart + 1, yend - ystart + 1);
            for (int y = ystart; y < yend + 1; y++)
            {
                for (int x = xstart; x < xend + 1; x++)
                {
                    newGrid.set(x - xstart, y - ystart, this[x, y]);
                }
            }
            return newGrid;
        }

        public void paste(Grid other, int xstart, int ystart)
        {
            for (int y = 0; y < other.height; y++)
            {
                for (int x = 0; x < other.width; x++)
                {
                    if (other[x, y] != 0)
                    {
                        set(x + xstart, y + ystart, other[x, y]);
                    }
                }
            }
        }
    }
}