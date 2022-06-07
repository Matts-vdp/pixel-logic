using System.Numerics;

using Raylib_cs;

namespace Game.Components
{
    // represents a grid of components converts the grid matrix to components using connected components
    public class Field : ComponentCreator
    {
        private readonly Grid _grid;            // contains wich block is placed where

        public ComponentList CList;      // stores the mapping of block types to components

        public State State;
        // used to create new grid
        public Field(int w, int h, IFile file) : this(w, h, new ComponentList(file)) { }

        public Field(Grid grid, ComponentList list)
        {
            this.CList = list;
            this._grid = grid;
            State = new State(grid.Width, grid.Height);
            OffColor = Color.GREEN;
            OnColor = Color.GREEN;
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
            this.Name = Path.GetFileName(name);
            SaveData save = SaveData.FromJson(txt);
            _grid = new Grid(save.Width, save.Height, save.FromArray());
            CList = save.ReadComponents(_grid.Matrix, file);
            State = new State(save.Width, save.Height);
            OffColor = Color.GREEN;
            OnColor = Color.GREEN;
        }
        // saves grid to saves/circuit/filename
        public void Save(string filename, IFile file)
        {
            file.WriteAllTextAsync("saves/circuit/" + filename, ToSave().ToJson());
        }
        // loads grid from saves/circuit/save.json
        public void Load(string filename, string txt, IFile file)
        {
            Clear();
            Paste(new Field(filename, txt, file));
        }

        // creates a saveData object from this grid object
        private SaveData ToSave()
        {
            return new SaveData(_grid.Width, _grid.Height, _grid.Matrix, CList.Custom);
        }

        // converts world coordinates to grid coordinates
        public static int ToGrid(float pos, int gridsize, int off)
        {
            pos = (pos + off) / gridsize;
            return (int)pos;
        }
        //------------------------------------------------------------------

        // CHANGE GRID

        public bool Add(Vector2 pos, int t, int gridsize, int xoff, int yoff)
        {
            int x = ToGrid(pos.X, gridsize, xoff);
            int y = ToGrid(pos.Y, gridsize, yoff);
            bool change = _grid.Set(x, y, t);
            if (change)
                State.SetState(new Pos(x, y), false);
                State.DrawText.Remove(new Pos(x,y));
            return change;
        }

        // remove block from grid
        public bool Del(Vector2 pos, int gridsize, int xoff, int yoff)
        {
            int x = ToGrid(pos.X, gridsize, xoff);
            int y = ToGrid(pos.Y, gridsize, yoff);
            bool change = _grid.Set(x, y, (int)Types.NONE);
            if (change)
                State.SetState(new Pos(x, y), false);
                State.DrawText.Remove(new Pos(x,y));
            return change;
        }

        // clear grid
        private void Clear()
        {
            _grid.Clear();
            State = new State(_grid.Width, _grid.Height);
        }
        //------------------------------------------------------------------

        // CONVERT TO COMPONENTS
        // create components from grid matrix

        public Circuit BuildNewObjects()
        {
            return BuildObjects(new State(_grid.Width, _grid.Height));
        }
        public Circuit BuildObjects(CancellationToken ct)
        {
            return BuildObjects(State, ct);
        }
        public Circuit BuildObjects(State state, CancellationToken? ct = null)
        {
            Dictionary<int, Component> components = new();
            List<ButtonComp> buttons = new();
            List<ClockComp> clocks = new();
            List<Connection> connections = new();

            if (ct is CancellationToken token1)
                token1.ThrowIfCancellationRequested();
            int[,] labels = _grid.ConnectedComponents(ct);

            if (ct is CancellationToken token2)
                token2.ThrowIfCancellationRequested();
            CrossConnect(labels, connections, state, ct);

            if (ct is CancellationToken token3)
                token3.ThrowIfCancellationRequested();
            MakeComponents(labels, components, buttons, clocks, state, ct);

            if (ct is CancellationToken token4)
                token4.ThrowIfCancellationRequested();
            MakeConnections(labels, components, connections, state, ct);

            return new Circuit(Name, components, buttons, clocks, connections);
        }

        // connect wires with a cross connection between them
        private void CrossConnect(int[,] labels, List<Connection> connections, State state, CancellationToken? ct)
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    if (ct is CancellationToken token)
                        token.ThrowIfCancellationRequested();
                    if (labels[x, y] != -2) { continue; }
                    Connection c = CList.NewConnection(_grid[x, y], new Pos(x, y), state);
                    connections.Add(c);
                    if (_grid[x - 1, y] == (int)Types.WIRE && _grid[x + 1, y] == (int)Types.WIRE)
                    {
                        Grid.ChangeLabel(labels[x+1, y], labels[x - 1, y], labels, _grid.Width, _grid.Height);
                    }
                    if (_grid[x, y - 1] == (int)Types.WIRE && _grid[x, y + 1] == (int)Types.WIRE)
                    {
                        Grid.ChangeLabel(labels[x, y + 1], labels[x, y - 1], labels, _grid.Width, _grid.Height);
                    }
                }
            }
        }
        // create the components from the grid
        private void MakeComponents(
            int[,] labels, 
            Dictionary<int, Component> components, 
            List<ButtonComp> buttons, 
            List<ClockComp> clocks,
            State state, 
            CancellationToken? ct
            )
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    if (ct is CancellationToken token)
                        token.ThrowIfCancellationRequested();
                    if (labels[x, y] <= 0 || _grid[x, y] == 0) { continue; }
                    if (components.ContainsKey(labels[x, y]))
                    {
                        components[labels[x, y]].Add(new Pos(x, y));
                    }
                    else
                    {
                        Component c = CList.NewComponent(_grid[x, y], state);
                        c.Add(new Pos(x, y));
                        if (_grid[x, y] == (int)Types.BUT)
                        {
                            state.DrawText[new Pos(x,y)] = (buttons.Count+1).ToString();
                            buttons.Add((ButtonComp)c);
                        }
                        if (_grid[x, y] == (int)Types.CLK)
                        {
                            clocks.Add((ClockComp)c);
                        }
                        components.Add(labels[x, y], c);
                    }
                }
            }
        }
        // create the connections between components
        private void MakeConnections(int[,] labels, Dictionary<int, Component> components, List<Connection> connections, State state, CancellationToken? ct)
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    if (ct is CancellationToken token)
                        token.ThrowIfCancellationRequested();
                    if (labels[x, y] != -1) { continue; }
                    bool wiref = false;
                    bool otherf = false;
                    Pos[] neighbors = { new Pos(x, y - 1), new Pos(x - 1, y), new Pos(x, y + 1), new Pos(x + 1, y) };
                    Connection con = CList.NewConnection(_grid[x, y], new Pos(x, y), state);
                    foreach (Pos pos in neighbors)
                    {
                        int block = _grid[pos.X, pos.Y];
                        if (block == 0) { continue; }
                        if (!wiref && block == (int)Types.WIRE)
                        {
                            con.AddWire(components[labels[pos.X, pos.Y]]);
                            wiref = true;
                        }
                        else if (!otherf && labels[pos.X, pos.Y] >= 0 && block != (int)Types.WIRE)
                        {
                            con.AddOther(components[labels[pos.X, pos.Y]]);
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
        public Field Copy(int xstart, int ystart, int xend, int yend)
        {
            Grid newGrid = _grid.Copy(xstart, ystart, xend, yend);
            Field newField = new(newGrid, CList)
            {
                Name = Name
            };
            return newField;
        }
        // copy and remove in original
        public Field Cut(int xstart, int ystart, int xend, int yend)
        {
            Field newField = Copy(xstart, ystart, xend, yend);
            _grid.Clear(xstart, ystart, xend, yend);
            return newField;
        }
        // merge customcomponents of componentlist adds missing components
        // and changes references to already imported components 
        private void MergeComponents(Field other)
        {
            foreach (int key in other.CList.Custom.Keys)
            {
                int index = CList.Add(other.CList.Custom[key].Name);
                Grid.ChangeLabel(key, index, other._grid.Matrix, other._grid.Width, other._grid.Height);
            }
        }
        // paste other grid into this grid
        public void Paste(Field other, Vector2 pos, int gridsize, int xoff, int yoff)
        {
            // add custom components
            MergeComponents(other);
            int x = ToGrid(pos.X, gridsize, xoff);
            int y = ToGrid(pos.Y, gridsize, yoff);
            _grid.Paste(other._grid, x, y);
        }
        // paste other grid in this grid at 0, 0
        private void Paste(Field other)
        {
            Paste(other, Vector2.Zero, 1, 0, 0);
        }

        //------------------------------------------------------------------
        // CUSTOM COMPONENT
        // creates a subcomponent from this grid
        public override Component CreateComponent(State state)
        {
            state = new State(_grid.Width, _grid.Height);
            return BuildObjects(state).ToComponent(state);
        }
        //------------------------------------------------------------------

        // draw all components
        public void Draw(int gridsize, int xoff, int yoff)
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    if (_grid[x, y] != 0)
                    {
                        int xpos = x * gridsize - xoff;
                        int ypos = y * gridsize - yoff;
                        CList.Draw(_grid[x, y], xpos, ypos, gridsize, State.GetState(new Pos(x, y)));
                    }
                }
            }
            foreach (Pos p in State.DrawText.Keys)
            {
                int xpos = p.X * gridsize - xoff;
                int ypos = p.Y * gridsize - yoff;
                Raylib.DrawText(State.DrawText[p], xpos+gridsize/3, ypos, gridsize, Color.BLACK);
            }
        }
        public override void Draw(int x, int y, int gridsize, bool state)
        {
            Color color = state ? OnColor : OffColor;
            Raylib.DrawRectangle(x, y, gridsize, gridsize, color);
            Raylib.DrawText(Name[0].ToString(), x + gridsize / 3, y, gridsize, Color.BLACK);
        }
    }
}