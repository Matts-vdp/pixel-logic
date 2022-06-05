using System.Numerics;

using Game.Components;

using Raylib_cs;

namespace Game
{
    // contains all data of the game
    public class Simulation
    {
        private int _gridsize = 32;   // size of grid cells
        private int _selected = 1;    // selected item of the list on the left
        private int _xoff = 0;        // camera offset x
        private int _yoff = 0;        // camera offset y
        private int _xsel = -1;       // selection start x
        private int _ysel = -1;       // selection start y
        private Field? _cloneGrid;            // stores grid after copy
        private Circuit? _cloneCircuit;
        private readonly Field _grid;                 // main grid
        private Circuit _circuit;            // build components
        private string _filename = "";       // remembers last dragged filename
        private double _time = 0;
        private const double DELAY = 0.2;
        private bool _rebuild = false;

        private readonly IFile _file;

        public Simulation(IFile file)
        {
            this._file = file;
            _grid = new Field(200, 200, file);
            _circuit = _grid.BuildObjects();
        }

        // processes all mouse input
        private bool InputMouse()
        {
            bool rebuild = false;
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                if (_grid.Add(pos, _selected, _gridsize, _xoff, _yoff))
                    rebuild = true;
            }
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                if (_grid.Del(pos, _gridsize, _xoff, _yoff))
                    rebuild = true;
            }
            _selected -= (int)Raylib.GetMouseWheelMove();
            int max = _grid.CList.Count;
            if (_selected > max)
            {
                _selected = max;
            }
            if (_selected < 1) { _selected = 1; }
            return rebuild;
        }

        // processes all keyboard input
        private bool InputKeyboard()
        {
            bool rebuild = false;
            //zoom
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_ADD))
            {
                _xoff = (int)(_xoff / (double)_gridsize) * (_gridsize + 1);
                _yoff = (int)(_yoff / (double)_gridsize) * (_gridsize + 1);
                _gridsize += 1;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_SUBTRACT))
            {
                if (_gridsize <= 1)
                {
                    _gridsize = 1;
                }
                else
                {
                    _xoff = (int)(_xoff / (double)_gridsize) * (_gridsize - 1);
                    _yoff = (int)(_yoff / (double)_gridsize) * (_gridsize - 1);
                    _gridsize -= 1;
                }
            }
            // camera movement
            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
            {
                _yoff += 4;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
            {
                _yoff -= 4;
                if (_yoff < 0) { _yoff = 0; }
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
            {
                _xoff += 4;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
            {
                _xoff -= 4;
                if (_xoff < 0) { _xoff = 0; }
            }
            // save and load
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
            {
                _grid.Save("save.json", _file);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
            {
                string filename = "saves/circuit/save.json";
                string txt = File.ReadAllText(filename);
                _grid.Load(filename, txt, _file);
            }
            // copy paste
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_C) || Raylib.IsKeyPressed(KeyboardKey.KEY_X))
            {
                Vector2 pos = Raylib.GetMousePosition();
                _xsel = Field.ToGrid(pos.X, _gridsize, _xoff);
                _ysel = Field.ToGrid(pos.Y, _gridsize, _yoff);
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_C) || Raylib.IsKeyDown(KeyboardKey.KEY_X))
            {
                Vector2 mpos = Raylib.GetMousePosition();
                int x = (int)(mpos.X + _xoff) / _gridsize;
                int y = (int)(mpos.Y + _yoff) / _gridsize;
                int xstart = Math.Min(_xsel, x);
                int ystart = Math.Min(_ysel, y);
                Raylib.DrawRectangle(
                    xstart * _gridsize - _xoff,
                    ystart * _gridsize - _yoff,
                    (Math.Abs(x - _xsel) + 1) * _gridsize,
                    (Math.Abs(y - _ysel) + 1) * _gridsize,
                    new Color(255, 255, 255, 50)
                );

            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_C))
            {
                Vector2 pos = Raylib.GetMousePosition();
                int xend = Field.ToGrid(pos.X, _gridsize, _xoff);
                int yend = Field.ToGrid(pos.Y, _gridsize, _yoff);
                _cloneGrid = _grid.Copy(Math.Min(_xsel, xend), Math.Min(_ysel, yend), Math.Max(_xsel, xend), Math.Max(_ysel, yend));
                _xsel = -1; _ysel = -1;
                _cloneGrid.Save("clipboard.json", _file);
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_X))
            {
                Vector2 pos = Raylib.GetMousePosition();
                int xend = Field.ToGrid(pos.X, _gridsize, _xoff);
                int yend = Field.ToGrid(pos.Y, _gridsize, _yoff);
                _cloneGrid = _grid.Cut(Math.Min(_xsel, xend), Math.Min(_ysel, yend), Math.Max(_xsel, xend), Math.Max(_ysel, yend));
                _xsel = -1; _ysel = -1;
                _cloneGrid.Save("clipboard.json", _file);
                rebuild = true;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_V))
            {
                _cloneCircuit = _cloneGrid?.BuildObjects();
                DrawCloneGrid();
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_V))
            {
                if (_cloneGrid != null)
                {
                    Vector2 pos = Raylib.GetMousePosition();
                    _grid.Paste(_cloneGrid, pos, _gridsize, _xoff, _yoff);
                    rebuild = true;
                }
            }
            // create subcomponent
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_I))
            {
                _grid.CList.Add(_filename);
            }
            return rebuild;
        }

        // handle input
        public void Input()
        {
            bool rebuild = InputMouse();
            rebuild = InputKeyboard() || rebuild;
            if (rebuild)
            {
                this._rebuild = true;
                _time = Raylib.GetTime();
            }
            _circuit.Input(); // handle extra input of grid
        }

        // draws game UI
        private void DrawUI()
        {
            for (int i = 0; i < _grid.CList.Count; i++)
            {
                bool sel = (i + 1) == _selected;
                Raylib.DrawText(_grid.CList.GetName(i + 1), 20, 20 * (i + 1), 20, sel ? Color.WHITE : Color.GRAY);
            }
            Raylib.DrawFPS(Raylib.GetScreenWidth() - 80, 0);
        }

        // display the gridcell selected by the mouse
        private void DrawMouse()
        {
            Vector2 mpos = Raylib.GetMousePosition();
            int x = (int)(mpos.X + _xoff) / _gridsize;
            int y = (int)(mpos.Y + _yoff) / _gridsize;
            Raylib.DrawRectangle(x * _gridsize - _xoff, y * _gridsize - _yoff, _gridsize, _gridsize, new Color(255, 255, 255, 25));
        }

        // display the selection rectangle when copying
        private void DrawCloneGrid()
        {
            if (_cloneGrid == null) return;
            Vector2 mpos = Raylib.GetMousePosition();
            int x = (int)(mpos.X + _xoff) / _gridsize;
            int y = (int)(mpos.Y + _yoff) / _gridsize;
            _cloneGrid?.Draw(_gridsize, (-x * _gridsize) + _xoff, (-y * _gridsize) + _yoff);
        }

        // handle game update
        public void Update()
        {
            if (_rebuild && Raylib.GetTime() - _time > DELAY)
            {
                _circuit = _grid.BuildObjects();
                _rebuild = false;
            }
            _circuit.Update();
        }
        // draw gameobjects
        public void Draw()
        {
            _grid.Draw(_gridsize, _xoff, _yoff);
            DrawMouse();
            DrawUI();
        }

        // handle files being dragged in
        public void Filecheck()
        {
            if (Raylib.IsFileDropped())
            {
                string[] files = Raylib.GetDroppedFiles();
                if (Raylib.IsFileExtension(files[0], ".json"))
                {
                    string txt = File.ReadAllText(files[0]);
                    _cloneGrid = new Field(files[0], txt, _file);
                    _filename = files[0];
                }
                else if (Raylib.IsFileExtension(files[0], ".cpl") || Raylib.IsFileExtension(files[0], ".ppl"))
                {
                    string name = Path.GetFileName(files[0]);
                    _grid.CList.Add(name);
                }
                Raylib.ClearDroppedFiles();
            }
        }
    }
}