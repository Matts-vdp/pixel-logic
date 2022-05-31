using Raylib_cs;
using System.Numerics;

using Game.Components;

namespace Game
{
    // contains all data of the game
    public class Simulation
    {
        private static int GRIDSIZE = 32;   // size of grid cells
        private static int selected = 1;    // selected item of the list on the left
        private static int xoff = 0;        // camera offset x
        private static int yoff = 0;        // camera offset y
        private static int xsel = -1;       // selection start x
        private static int ysel = -1;       // selection start y
        private Grid? cloneGrid;            // stores grid after copy
        private Circuit? cloneCircuit;
        private Grid grid = new Grid(200, 200); // main grid
        private Circuit circuit;                // build components
        private string filename = "";       // remembers last dragged filename
        private double time = 0;
        private const double DELAY = 0.5;
        private bool rebuild = false;
        public Simulation()
        {
            circuit = grid.buildObjects();
        }

        // processes all mouse input
        private bool InputMouse()
        {
            bool rebuild = false;
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                if (grid.add(pos, selected, GRIDSIZE, xoff, yoff))
                    rebuild = true;
            }
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                if (grid.del(pos, GRIDSIZE, xoff, yoff))
                    rebuild = true;
            }
            selected -= (int)Raylib.GetMouseWheelMove();
            int max = grid.list.Count;
            if (selected > max)
            {
                selected = max;
            }
            if (selected < 1) { selected = 1; }
            return rebuild;
        }

        // processes all keyboard input
        private bool InputKeyboard()
        {
            bool rebuild = false;
            //zoom
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_ADD))
            {
                xoff = (int)(xoff / (double)GRIDSIZE) * (GRIDSIZE + 1);
                yoff = (int)(yoff / (double)GRIDSIZE) * (GRIDSIZE + 1);
                GRIDSIZE += 1;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_SUBTRACT))
            {
                if (GRIDSIZE <= 0)
                {
                    GRIDSIZE = 1;
                }
                else
                {
                    xoff = (int)(xoff / (double)GRIDSIZE) * (GRIDSIZE - 1);
                    yoff = (int)(yoff / (double)GRIDSIZE) * (GRIDSIZE - 1);
                    GRIDSIZE -= 1;
                }
            }
            // camera movement
            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
            {
                yoff += 4;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
            {
                yoff -= 4;
                if (yoff < 0) { yoff = 0; }
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
            {
                xoff += 4;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
            {
                xoff -= 4;
                if (xoff < 0) { xoff = 0; }
            }
            // save and load
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
            {
                grid.save("save.json");
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
            {
                grid.load(GRIDSIZE);
            }
            // copy paste
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_C) || Raylib.IsKeyPressed(KeyboardKey.KEY_X))
            {
                Vector2 pos = Raylib.GetMousePosition();
                xsel = Grid.toGrid(pos.X, GRIDSIZE, xoff);
                ysel = Grid.toGrid(pos.Y, GRIDSIZE, yoff);
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_C) || Raylib.IsKeyDown(KeyboardKey.KEY_X))
            {
                Vector2 mpos = Raylib.GetMousePosition();
                int x = (int)(mpos.X + xoff) / GRIDSIZE;
                int y = (int)(mpos.Y + yoff) / GRIDSIZE;
                int xstart = Math.Min(xsel, x);
                int ystart = Math.Min(ysel, y);
                Raylib.DrawRectangle(
                    xstart * GRIDSIZE - xoff,
                    ystart * GRIDSIZE - yoff,
                    (Math.Abs(x - xsel) + 1) * GRIDSIZE,
                    (Math.Abs(y - ysel) + 1) * GRIDSIZE,
                    new Color(255, 255, 255, 50)
                );

            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_C))
            {
                Vector2 pos = Raylib.GetMousePosition();
                int xend = Grid.toGrid(pos.X, GRIDSIZE, xoff);
                int yend = Grid.toGrid(pos.Y, GRIDSIZE, yoff);
                cloneGrid = grid.copy(Math.Min(xsel, xend), Math.Min(ysel, yend), Math.Max(xsel, xend), Math.Max(ysel, yend));
                xsel = -1; ysel = -1;
                cloneGrid.save("clipboard.json");
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_X))
            {
                Vector2 pos = Raylib.GetMousePosition();
                int xend = Grid.toGrid(pos.X, GRIDSIZE, xoff);
                int yend = Grid.toGrid(pos.Y, GRIDSIZE, yoff);
                cloneGrid = grid.cut(Math.Min(xsel, xend), Math.Min(ysel, yend), Math.Max(xsel, xend), Math.Max(ysel, yend));
                xsel = -1; ysel = -1;
                cloneGrid.save("clipboard.json");
                rebuild = true;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_V))
            {
                cloneCircuit = cloneGrid?.buildObjects();
                drawCloneGrid();
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_V))
            {
                if (cloneGrid != null)
                {
                    Vector2 pos = Raylib.GetMousePosition();
                    grid.paste(cloneGrid, pos, GRIDSIZE, xoff, yoff);
                    rebuild = true;
                }
            }
            // create subcomponent
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_I))
            {
                grid.list.add(filename);
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
                this.rebuild = true;
                time = Raylib.GetTime();
            }
            circuit.Input(); // handle extra input of grid
        }

        // draws game UI
        private void drawUI()
        {
            for (int i = 0; i < grid.list.Count; i++)
            {
                bool sel = (i + 1) == selected;
                Raylib.DrawText(grid.list.getName(i), 20, 20 * (i + 1), 20, sel ? Color.WHITE : Color.GRAY);
            }
            Raylib.DrawFPS(Raylib.GetScreenWidth() - 80, 0);
        }

        // display the gridcell selected by the mouse
        private void drawMouse()
        {
            Vector2 mpos = Raylib.GetMousePosition();
            int x = (int)(mpos.X + xoff) / GRIDSIZE;
            int y = (int)(mpos.Y + yoff) / GRIDSIZE;
            Raylib.DrawRectangle(x * GRIDSIZE - xoff, y * GRIDSIZE - yoff, GRIDSIZE, GRIDSIZE, new Color(255, 255, 255, 25));
        }

        // display the selection rectangle when copying
        private void drawCloneGrid()
        {
            if (cloneGrid == null) return;
            Vector2 mpos = Raylib.GetMousePosition();
            int x = (int)(mpos.X + xoff) / GRIDSIZE;
            int y = (int)(mpos.Y + yoff) / GRIDSIZE;
            cloneGrid?.draw(GRIDSIZE, (-x * GRIDSIZE) + xoff, (-y * GRIDSIZE) + yoff);
        }

        // handle game update
        public void update()
        {
            if (rebuild && Raylib.GetTime()-time > DELAY)
            {
                circuit = grid.buildObjects();
                rebuild = false;
            }
            circuit.update();
        }
        // draw gameobjects
        public void draw()
        {
            grid.draw(GRIDSIZE, xoff, yoff);
            drawMouse();
            drawUI();
        }

        // handle files being dragged in
        public void filecheck()
        {
            if (Raylib.IsFileDropped())
            {
                string[] files = Raylib.GetDroppedFiles();
                if (Raylib.IsFileExtension(files[0], ".json"))
                {
                    cloneGrid = new Grid(files[0]);
                    filename = files[0];
                }
                else if (Raylib.IsFileExtension(files[0], ".cpl") || Raylib.IsFileExtension(files[0], ".ppl"))
                {
                    string name = Path.GetFileName(files[0]);
                    grid.list.add(name);
                }
                Raylib.ClearDroppedFiles();
            }
        }
    }
}