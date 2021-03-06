using System.Numerics;

using Game.Components;

using Raylib_cs;

namespace Game
{
    // contains all data of the game
    public class Simulation
    {
        private int _gridsize = 32;   // size of grid cells
        private int _xoff = 0;        // camera offset x
        private int _yoff = 0;        // camera offset y
        private int _xsel = -1;       // selection start x
        private int _ysel = -1;       // selection start y
        private Field? _cloneGrid;            // stores grid after copy
        private readonly Field _grid;                 // main grid
        private Circuit _circuit;            // build components
        private string _filename = "";       // remembers last dragged filename
        private double _time = 0;
        private const double DELAY = 0.2;
        private bool _rebuild = false;
        private CancellationTokenSource _token;
        private readonly IFile _file;
        private int _updateDelay = 10;
        private const int MaxUpdateDelay = 10000;
        private int UpdateDelay
        {
            get
            {
                return _updateDelay;
            }
            set
            {
                _updateDelay = value;
                if (_updateDelay > MaxUpdateDelay)
                    _updateDelay = MaxUpdateDelay;
                else if (_updateDelay < 0)
                    _updateDelay = 0;
            }
        }
        private int _clockDelay = 1000;
        private const int MaxClockDelay = 10000;
        private int ClockDelay
        {
            get
            {
                return _clockDelay;
            }
            set
            {
                _clockDelay = value;
                if (_clockDelay > MaxClockDelay)
                    _clockDelay = MaxClockDelay;
                else if (_clockDelay < 0)
                    _clockDelay = 0;
            }
        }

        private int _sel = 1;
        private int Selected
        {
            get
            {
                return _sel;
            }
            set
            {
                _sel = value;
                if (_sel > _grid.CList.Count)
                    _sel = _grid.CList.Count;
                else if (_sel < 1)
                    _sel = 1;
            }
        }

        private Task? _updater;
        private string _simstate = "SIM";

        public Simulation(IFile file)
        {
            _file = file;
            _grid = new Field(200, 200, file);
            _circuit = _grid.BuildNewObjects();
            _token = new();
        }

        // processes all mouse input
        private bool InputMouse()
        {
            bool rebuild = false;
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                if (_grid.Add(pos, Selected, _gridsize, _xoff, _yoff))
                    rebuild = true;
            }
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                if (_grid.Del(pos, _gridsize, _xoff, _yoff))
                    rebuild = true;
            }
            Selected -= (int)Raylib.GetMouseWheelMove();
            return rebuild;
        }

        // processes all keyboard input
        private bool InputKeyboard()
        {
            bool rebuild = false;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_T))
            {
                Selected -= 1;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_G))
            {
                Selected += 1;
            }
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
                rebuild = true;
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
                string name = Path.GetFileName(_filename);
                _grid.CList.Add(name);
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_U))
            {
                UpdateDelay += 2;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_J))
            {
                UpdateDelay -= 2;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_Y))
            {
                ClockDelay += 2;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_H))
            {
                ClockDelay -= 2;
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
                _rebuild = true;
                _time = Raylib.GetTime();
            }
            _circuit.Input(); // handle extra input of grid
        }

        // draws game UI
        private void DrawUI()
        {
            const int font = 20;
            const int xstart = 5;
            const int xstartName = 20;
            const int ystart = 20;
            const int rsize = 10;

            // draw components
            for (int i = 0; i < _grid.CList.Count; i++)
            {
                bool sel = (i + 1) == Selected;
                int ypos = ystart* (i + 1);
                string name = _grid.CList.GetName(i + 1);
                Color color = _grid.CList.GetColor(i+1, sel);
                if (sel)
                    Raylib.DrawRectangle(xstart, ypos+font/4, rsize, rsize, color);
                Raylib.DrawText(name, xstartName, ypos, font, color);
            }

            // draw info
            int xbegin = Raylib.GetScreenWidth() - 85;
            int xbeginText = xbegin - 85;
            int ybegin = 20;
            int fontsize = 20;
            int offset = 25;
            
            Raylib.DrawText("FPS:", xbeginText, ybegin, fontsize, Color.GRAY);
            Raylib.DrawText(Raylib.GetFPS().ToString(), xbegin, ybegin, fontsize, Color.WHITE);
            ybegin += offset;
            Raylib.DrawText("Update:", xbeginText, ybegin, fontsize, Color.GRAY);
            Raylib.DrawText(UpdateDelay.ToString()+"ms", xbegin, ybegin, fontsize, Color.WHITE);
            ybegin += offset;
            Raylib.DrawText("Clock:", xbeginText, ybegin, fontsize, Color.GRAY);
            Raylib.DrawText(ClockDelay.ToString()+"ms", xbegin, ybegin, fontsize, Color.WHITE);
            ybegin += offset;
            Raylib.DrawText("State:", xbeginText, ybegin, fontsize, Color.GRAY);
            Raylib.DrawText(_simstate, xbegin, ybegin, fontsize, Color.WHITE);
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
                _token.Cancel();
                _updater?.Wait();
                _token.Dispose();
                _token = new CancellationTokenSource();
                CancellationToken ct = _token.Token;

                _updater = Task.Run(() =>
                {
                    _simstate = "BUILD";
                    _circuit = _grid.BuildObjects(ct);
                    _simstate = "SIM";
                    while (!ct.IsCancellationRequested)
                    {
                        _circuit.Update(ct, ClockDelay);
                        if (UpdateDelay > 0)
                            ct.WaitHandle.WaitOne(UpdateDelay);
                    }
                    _simstate = "BUILD";
                }, ct);

                _rebuild = false;
            }
        }
        // draw gameobjects
        public void Draw()
        {
            _grid.Draw(_gridsize, _xoff, _yoff);
            DrawMouse();
            DrawUI();
        }

        public void Stop()
        {
            _token.Cancel();
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
                    name = Path.GetFileName(name);
                    _grid.CList.Add(name);
                }
                Raylib.ClearDroppedFiles();
            }
        }
    }
}