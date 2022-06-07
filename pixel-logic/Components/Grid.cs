using System.Numerics;

namespace Game.Components
{
    public class Grid
    {
        public int Width;
        public int Height;

        public int[,] Matrix;

        public Grid(int w, int h, int[,] g)
        {
            Width = w;
            Height = h;
            Matrix = g;
        }

        public Grid(int w, int h) : this(w, h, new int[w, h])
        {
        }

        public int this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return 0;
                return Matrix[x, y];
            }
        }

        // add block to grid with grid coordinates
        public bool Set(int x, int y, int t)
        {
            if (x < 0 || x >= Width) { return false; }
            if (y < 0 || y >= Height) { return false; }
            if (Matrix[x, y] == t) { return false; }
            Matrix[x, y] = t;
            return true;
        }

        public void Clear()
        {
            Clear(0, 0, Width, Height);
        }

        public void Clear(int xstart, int ystart, int xend, int yend)
        {
            for (int y = ystart; y < yend + 1; y++)
            {
                for (int x = xstart; x < xend + 1; x++)
                {
                    Set(x, y, (int)Types.NONE);
                }
            }
        }

        // changes all occurences of "from" to "to" in the grid
        public static int[,] ChangeLabel(int from, int to, int[,] labels, int width, int height)
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
        public int[,] ConnectedComponents(CancellationToken? ct = null)
        {
            int[,] labels = new int[Width, Height];
            int label = 1;
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (ct is CancellationToken token)
                            token.ThrowIfCancellationRequested();
                        if (Matrix[x, y] == 0) continue;
                        if (labels[x, y] == 0 && (Matrix[x, y] == (int)Types.OUT || Matrix[x, y] == (int)Types.IN || Matrix[x, y] == (int)Types.CLKIN))
                        {
                            labels[x, y] = -1;
                            changed = true;
                            continue;
                        }
                        if (labels[x, y] == 0 && Matrix[x, y] == (int)Types.CROSS)
                        {
                            labels[x, y] = -2;
                            changed = true;
                            continue;
                        }
                        bool found = false;
                        if (this[x - 1, y] == Matrix[x, y] && labels[x, y] != labels[x - 1, y])
                        {
                            labels[x, y] = labels[x - 1, y];
                            found = true;
                            changed = true;
                        }
                        if (this[x, y - 1] == Matrix[x, y] && labels[x, y] != labels[x, y - 1])
                        {
                            if (this[x - 1, y] == Matrix[x, y])
                            {
                                labels = Grid.ChangeLabel(labels[x, y], labels[x, y - 1], labels, Width, Height);
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

        public Grid Copy(int xstart, int ystart, int xend, int yend)
        {
            Grid newGrid = new(xend - xstart + 1, yend - ystart + 1);
            for (int y = ystart; y < yend + 1; y++)
            {
                for (int x = xstart; x < xend + 1; x++)
                {
                    newGrid.Set(x - xstart, y - ystart, this[x, y]);
                }
            }
            return newGrid;
        }

        public void Paste(Grid other, int xstart, int ystart)
        {
            for (int y = 0; y < other.Height; y++)
            {
                for (int x = 0; x < other.Width; x++)
                {
                    if (other[x, y] != 0)
                    {
                        Set(x + xstart, y + ystart, other[x, y]);
                    }
                }
            }
        }
    }
}