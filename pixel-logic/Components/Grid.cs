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

        public Grid(int w, int h) :this(w, h, new int[h,w])
        {
        }

        public int this[int x, int y] 
        {
            get 
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                    return 0;
                return grid[y, x];
            }
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

        public bool del(int x, int y)
        {
            if (x < 0 || x >= width) { return false; }
            if (y < 0 || y >= height) { return false; }
            if (grid[y, x] == 0) { return false; }
            grid[y, x] = 0;
            return true;
        }

        public void clear()
        {
            clear(0,0, width, height);
        }

        public void clear(int xstart, int ystart, int xend, int yend)
        {
            for (int y = ystart; y < yend + 1; y++)
            {
                for (int x = xstart; x < xend + 1; x++)
                {
                    if (y>=height || x>=width)
                        continue;
                    grid[y, x] = (int)types.NONE;
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
                    if (labels[y, x] == from)
                    {
                        labels[y, x] = to;
                    }
                }
            }
            return labels;
        }

        // use connected components algorithm to group blocks of the same type
        // that are connected to each other into the same component
        public int[,] connectedComponents()
        {
            int[,] labels = new int[height, width];
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
                        if (this[x - 1, y] == grid[y, x] && labels[y, x] != labels[y, x - 1])
                        {
                            labels[y, x] = labels[y, x - 1];
                            found = true;
                            changed = true;
                        }
                        if (this[x, y - 1] == grid[y, x] && labels[y, x] != labels[y - 1, x])
                        {
                            if (this[x - 1, y] == grid[y, x])
                            {
                                labels = Grid.changeLabel(labels[y, x], labels[y - 1, x], labels, width, height);
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
            return labels;
        }
    
        public Grid copy(int xstart, int ystart, int xend, int yend)
        {
            Grid newGrid = new Grid(xend - xstart + 1, yend - ystart + 1);
            for (int y = ystart; y < yend + 1; y++)
            {
                for (int x = xstart; x < xend + 1; x++)
                {
                    if (y>=height || x>=width)
                        continue;
                    newGrid.grid[y - ystart, x - xstart] = grid[y, x];
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
                    if (other[y, x] != 0)
                    {
                        add(x+xstart, y+ystart, other[y, x]);
                    }
                }
            }
        }
    }
}