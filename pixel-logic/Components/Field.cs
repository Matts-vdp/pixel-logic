using Raylib_cs;
using System.Numerics;

namespace Game.Components
{
    // represents a grid of components converts the grid matrix to components using connected components
    public class Field : ComponentCreator
    {
        private Grid grid;            // contains wich block is placed where

        public ComponentList list;      // stores the mapping of block types to components

        public State state;
        // used to create new grid
        public Field(int w, int h, IFile file) : this(w, h, new ComponentList(file)) { }

        public Field(Grid grid, ComponentList list)
        {
            this.list = list;
            this.grid = grid;
            state = new State(grid.width, grid.height);
            offColor = Color.GREEN;
            onColor = Color.GREEN;
        }

        // used to create new grid with same componentList 
        private Field(int w, int h, ComponentList list) : this(new Grid(w, h), list)
        {
        }
        //------------------------------------------------------------------

        // JSON
        // used to load a grid from a json file
        public Field(string name, string txt, IFile file)
        {
            // load json
            this.name = Path.GetFileName(name);
            SaveData save = SaveData.fromJson(txt);
            grid = new Grid(save.width, save.height, save.fromArray());
            list = save.readComponents(grid.grid, file);
            state = new State(save.width, save.height);
            offColor = Color.GREEN;
            onColor = Color.GREEN;
        }
        // saves grid to saves/circuit/filename
        public void save(string filename, IFile file)
        {
            file.WriteAllTextAsync("saves/circuit/" + filename, toSave().toJson());
        }
        // loads grid from saves/circuit/save.json
        public void load(string filename, string txt, IFile file)
        {
            clear();
            paste(new Field(filename, txt, file));
        }

        // creates a saveData object from this grid object
        private SaveData toSave()
        {
            return new SaveData(grid.width, grid.height, grid.grid, list.custom);
        }

        // converts world coordinates to grid coordinates
        public static int toGrid(float pos, int gridsize, int off)
        {
            pos = (pos + off) / gridsize;
            return (int)pos;
        }
        //------------------------------------------------------------------

        // CHANGE GRID

        public bool add(Vector2 pos, int t, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            bool change = grid.set(x, y, t);
            if (change)
                state.setState(new Pos(x, y), false);
            return change;
        }

        // remove block from grid
        public bool del(Vector2 pos, int gridsize, int xoff, int yoff)
        {
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            bool change = grid.set(x, y, (int)types.NONE);
            if (change)
                state.setState(new Pos(x, y), false);
            return change;
        }

        // clear grid
        private void clear()
        {
            grid.clear();
            state = new State(grid.width, grid.height);
        }
        //------------------------------------------------------------------

        // CONVERT TO COMPONENTS
        // create components from grid matrix
        public Circuit buildObjects()
        {
            Dictionary<int, Component> components = new Dictionary<int, Component>();
            List<ButtonComp> buttons = new List<ButtonComp>();
            List<Connection> connections = new List<Connection>();

            int[,] labels = grid.connectedComponents();
            crossConnect(labels, connections);
            makeComponents(labels, components, buttons);
            makeConnections(labels, components, connections);

            return new Circuit(name, components, buttons, connections);
        }

        // connect wires with a cross connection between them
        private void crossConnect(int[,] labels, List<Connection> connections)
        {
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    if (labels[x, y] != -2) { continue; }
                    Connection c = list.NewConnection(grid[x, y], new Pos(x, y), state);
                    connections.Add(c);
                    if (grid[x - 1, y] == (int)types.WIRE && grid[x + 1, y] == (int)types.WIRE)
                    {
                        Grid.changeLabel(labels[y, x + 1], labels[x - 1, y], labels, grid.width, grid.height);
                    }
                    if (grid[x, y - 1] == (int)types.WIRE && grid[x, y + 1] == (int)types.WIRE)
                    {
                        Grid.changeLabel(labels[x, y + 1], labels[x, y - 1], labels, grid.width, grid.height);
                    }
                }
            }
        }
        // create the components from the grid
        private void makeComponents(int[,] labels, Dictionary<int, Component> components, List<ButtonComp> buttons)
        {
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    if (labels[x, y] <= 0 || grid[x, y] == 0) { continue; }
                    if (components.ContainsKey(labels[x, y]))
                    {
                        components[labels[x, y]].add(new Pos(x, y));
                    }
                    else
                    {
                        Component c = list.NewComponent(grid[x, y], state);
                        c.add(new Pos(x, y));
                        if (grid[x, y] == (int)types.BUT)
                        {
                            buttons.Add((ButtonComp)c);
                        }
                        components.Add(labels[x, y], c);
                    }
                }
            }
        }
        // create the connections between components
        private void makeConnections(int[,] labels, Dictionary<int, Component> components, List<Connection> connections)
        {
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    if (labels[x, y] != -1) { continue; }
                    bool wiref = false;
                    bool otherf = false;
                    Pos[] neighbors = {new Pos(x, y - 1), new Pos(x - 1, y), new Pos(x, y + 1), new Pos(x + 1, y) };
                    Connection con = list.NewConnection(grid[x, y], new Pos(x, y), state);
                    foreach (Pos pos in neighbors)
                    {
                        int block = grid[pos.x, pos.y];
                        if (block == 0) { continue; }
                        if (!wiref && block == (int)types.WIRE)
                        {
                            con.addWire(components[labels[pos.x, pos.y]]);
                            wiref = true;
                        }
                        else if (!otherf && labels[pos.x, pos.y] >= 0 && block != (int)types.WIRE)
                        {
                            con.addOther(components[labels[pos.x, pos.y]]);
                            otherf = true;
                        }
                    }
                    connections.Add(con);
                }
            }
        }
        //------------------------------------------------------------------

        // COPY PASTE
        // copy part of grid to new grid object
        public Field copy(int xstart, int ystart, int xend, int yend)
        {
            Grid newGrid = grid.copy(xstart, ystart, xend, yend);
            Field newField = new Field(newGrid, list);
            newField.name = name;
            return newField;
        }
        // copy and remove in original
        public Field cut(int xstart, int ystart, int xend, int yend)
        {
            Field newField = copy(xstart, ystart, xend, yend);
            grid.clear(xstart, ystart, xend, yend);
            return newField;
        }
        // merge customcomponents of componentlist adds missing components
        // and changes references to already imported components 
        private void mergeComponents(Field other)
        {
            foreach (int key in other.list.custom.Keys)
            {
                int index = list.add(other.list.custom[key].name);
                Grid.changeLabel(key, index, other.grid.grid, other.grid.height, other.grid.width);
            }
        }
        // paste other grid into this grid
        public void paste(Field other, Vector2 pos, int gridsize, int xoff, int yoff)
        {
            // add custom components
            mergeComponents(other);
            int x = toGrid(pos.X, gridsize, xoff);
            int y = toGrid(pos.Y, gridsize, yoff);
            grid.paste(other.grid, x, y);
        }
        // paste other grid in this grid at 0, 0
        private void paste(Field other)
        {
            paste(other, Vector2.Zero, 1, 0, 0);
        }

        //------------------------------------------------------------------
        // CUSTOM COMPONENT
        // creates a subcomponent from this grid
        public override Component createComponent(State state)
        {
            return buildObjects().toComponent(state);
        }
        //------------------------------------------------------------------

        // draw all components
        public void draw(int gridsize, int xoff, int yoff)
        {
            for (int y = 0; y < grid.height; y++)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    if (grid[x, y] != 0)
                    {
                        int xpos = x * gridsize - xoff;
                        int ypos = y * gridsize - yoff; ;
                        list.draw(grid[x, y], xpos, ypos, gridsize, state.getState(new Pos(x, y)));
                    }
                }
            }
        }
        public override void draw(int x, int y, int gridsize, bool state)
        {
            Color color = state ? onColor : offColor;
            Raylib.DrawRectangle(x, y, gridsize, gridsize, color);
            Raylib.DrawText(name[0].ToString(), x + gridsize / 3, y, gridsize, Color.BLACK);
        }
    }
}