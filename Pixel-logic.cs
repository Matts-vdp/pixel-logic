using Raylib_cs;
using System.Numerics;



namespace game
{
    // contains the main game loop and all data of the game
    class Game
    {
        static int GRIDSIZE = 32;   // size of grid cells
        static int selected = 1;    // selected item of the list on the left
        static int xoff = 0;        // camera offset x
        static int yoff = 0;        // camera offset y
        static int xsel = -1;       // selection start x
        static int ysel = -1;       // selection start y
        Grid? cloneGrid;            // stores grid after copy
        Grid grid = new Grid(200, 200); // main grid
        string filename = "";       // remembers last dragged filename

        // processes all mouse input
        public void InputMouse()
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                grid.add(pos, selected, GRIDSIZE, xoff, yoff);
            }
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                grid.del(pos, GRIDSIZE, xoff, yoff);
            }
            selected -= (int)Raylib.GetMouseWheelMove();
            int max = grid.list.items.Count;
            if (selected > max)
            {
                selected = max;
            }
            if (selected < 1) { selected = 1; }
        }

        // processes all keyboard input
        public void InputKeyboard()
        {
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
                xsel = grid.toGrid(pos.X, GRIDSIZE, xoff);
                ysel = grid.toGrid(pos.Y, GRIDSIZE, yoff);
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
                int xend = grid.toGrid(pos.X, GRIDSIZE, xoff);
                int yend = grid.toGrid(pos.Y, GRIDSIZE, yoff);
                cloneGrid = grid.copy(Math.Min(xsel, xend), Math.Min(ysel, yend), Math.Max(xsel, xend), Math.Max(ysel, yend));
                xsel = -1; ysel = -1;
                cloneGrid.save("clipboard.json");
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_X))
            {
                Vector2 pos = Raylib.GetMousePosition();
                int xend = grid.toGrid(pos.X, GRIDSIZE, xoff);
                int yend = grid.toGrid(pos.Y, GRIDSIZE, yoff);
                cloneGrid = grid.cut(Math.Min(xsel, xend), Math.Min(ysel, yend), Math.Max(xsel, xend), Math.Max(ysel, yend));
                xsel = -1; ysel = -1;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_V))
            {
                drawCloneGrid();
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_V))
            {
                if (cloneGrid != null)
                {
                    Vector2 pos = Raylib.GetMousePosition();
                    grid.paste(cloneGrid, pos, GRIDSIZE, xoff, yoff);
                    return;
                }
            }
            // create subcomponent
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_O))
            {
                grid.list.add(filename);
            }
        }

        // handle input
        public void Input()
        {
            InputMouse();
            InputKeyboard();
            grid.Input(); // handle extra input of grid
        }

        // draws game UI
        public void drawUI()
        {
            for (int i = 0; i < grid.list.items.Count; i++)
            {
                bool sel = (i + 1) == selected;
                Raylib.DrawText(grid.list.items[i], 20, 20 * (i + 1), 20, sel ? Color.WHITE : Color.GRAY);
            }
            Raylib.DrawFPS(Raylib.GetScreenWidth() - 80, 0);
        }

        // display the gridcell selected by the mouse
        public void drawMouse()
        {
            Vector2 mpos = Raylib.GetMousePosition();
            int x = (int)(mpos.X + xoff) / GRIDSIZE;
            int y = (int)(mpos.Y + yoff) / GRIDSIZE;
            Raylib.DrawRectangle(x * GRIDSIZE - xoff, y * GRIDSIZE - yoff, GRIDSIZE, GRIDSIZE, new Color(255, 255, 255, 25));
        }

        // display the selection rectangle when copying
        public void drawCloneGrid()
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
            grid.update();
        }
        // draw gameobjects
        public void draw()
        {
            grid.draw(GRIDSIZE, xoff, yoff);
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

        // main game loop
        public static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(800, 800, "Pixel Logic");
            Raylib.SetWindowMinSize(300, 300);
            Raylib.SetTargetFPS(60);
            Game gm = new Game();

            while (!Raylib.WindowShouldClose())
            {
                // init screen
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);

                // check input
                gm.Input();
                gm.filecheck();

                // update objects
                gm.update();

                // draw
                gm.draw();
                gm.drawMouse();
                gm.drawUI();
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}