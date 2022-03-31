using Raylib_cs;
using System.Numerics;



namespace game
{
    static class Game
    {
        static int GRIDSIZE = 32;
        static int selected = 1;
        static int xoff = 0;
        static int yoff = 0;
        static int xsel = -1;
        static int ysel = -1;

        static Grid? cloneGrid;

        public static void Input(Grid grid)
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
            int max = ComponentFactory.items.Count;
            if (selected > max)
            {
                selected = max;
            }
            if (selected < 1) { selected = 1; }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_ADD))
            {
                GRIDSIZE += 1;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_SUBTRACT))
            {
                GRIDSIZE -= 1;
                if (GRIDSIZE <= 0) GRIDSIZE = 1;
            }
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
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
            {
                save(grid, "save.dpl");
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
            {
                load(grid);
            }
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
                int xstart = Math.Min(xsel,x); 
                int ystart = Math.Min(ysel,y); 
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
                save(cloneGrid, "clipboard.dpl");
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
                    grid.merge(cloneGrid, pos, GRIDSIZE, xoff, yoff);
                    return;
                }
            }
        }

        public static void drawUI()
        {
            for (int i = 0; i < ComponentFactory.items.Count; i++)
            {
                bool sel = (i + 1) == selected;
                Raylib.DrawText(ComponentFactory.items[i], 20, 20 * (i + 1), 20, sel ? Color.WHITE : Color.GRAY);
            }
            Raylib.DrawFPS(Raylib.GetScreenWidth() - 80, 0);
        }

        public static void drawMouse()
        {
            Vector2 mpos = Raylib.GetMousePosition();
            int x = (int)(mpos.X + xoff) / GRIDSIZE;
            int y = (int)(mpos.Y + yoff) / GRIDSIZE;
            Raylib.DrawRectangle(x * GRIDSIZE - xoff, y * GRIDSIZE - yoff, GRIDSIZE, GRIDSIZE, new Color(255, 255, 255, 25));
        }

        public static void drawCloneGrid()
        {
            if (cloneGrid == null) return;
            Vector2 mpos = Raylib.GetMousePosition();
            int x = (int)(mpos.X + xoff) / GRIDSIZE;
            int y = (int)(mpos.Y + yoff) / GRIDSIZE;
            cloneGrid?.draw(GRIDSIZE, (-x * GRIDSIZE) + xoff, (-y * GRIDSIZE) + yoff);
        }

        public static void save(Grid grid, string filename)
        {
            File.WriteAllTextAsync(filename, grid.toText());
        }
        public static void load(Grid grid)
        {
            string txt = File.ReadAllText("save.dpl");
            grid.clear();
            grid.mergeZero(new Grid(txt));
        }

        public static void loadCs(string filename) {
            CCode c = new CCode(filename);
            List<bool> inp = new List<bool>();
            inp.Add(true);
            List<bool> alp = c.run(inp);
        }        

        public static void filecheck()
        {
            if (Raylib.IsFileDropped())
            {
                string[] files = Raylib.GetDroppedFiles();
                string txt = File.ReadAllText(files[0]);
                if (Raylib.IsFileExtension(files[0], ".dpl")) {
                    cloneGrid = new Grid(txt);
                }
                else if (Raylib.IsFileExtension(files[0], ".cpl") || Raylib.IsFileExtension(files[0], ".ppl")) {
                    string name = Path.GetFileName(files[0]);
                    Codes.add(name);
                }
                Raylib.ClearDroppedFiles();
            }
        }
        public static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(800, 800, "Pixel Logic");
            Raylib.SetWindowMinSize(300, 300);
            Raylib.SetTargetFPS(60);

            Grid grid = new Grid(200, 200);

            while (!Raylib.WindowShouldClose())
            {
                // init screen
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);
                // check mouse and keyboard input
                Input(grid);
                grid.Input();
                filecheck();
                // update objects
                grid.update();
                // draw
                grid.draw(GRIDSIZE, xoff, yoff);
                drawMouse();
                drawUI();
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}