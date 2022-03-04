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

        public static void Input(Grid grid)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                grid.add(pos, (types)selected, GRIDSIZE, xoff, yoff);
            }
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                Vector2 pos = Raylib.GetMousePosition();
                grid.del(pos, GRIDSIZE, xoff, yoff);
            }
            selected -= (int)Raylib.GetMouseWheelMove();
            int max = Enum.GetValues(typeof(types)).Cast<int>().Max();
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
        }

        public static void drawUI()
        {
            for (int i = 0; i < ComponentFactory.items.Length; i++)
            {
                bool sel = (i + 1) == selected;
                Raylib.DrawText(ComponentFactory.items[i], 20, 20 * (i + 1), 20, sel ? Color.WHITE : Color.GRAY);
            }
            Raylib.DrawFPS(Raylib.GetScreenWidth()-80, 0);
        }

        public static void drawMouse(){
            Vector2 mpos = Raylib.GetMousePosition();
            int x = (int)(mpos.X + xoff) / GRIDSIZE;
            int y = (int)(mpos.Y + yoff) / GRIDSIZE;
            Raylib.DrawRectangle(x * GRIDSIZE - xoff, y * GRIDSIZE - yoff, GRIDSIZE, GRIDSIZE, new Color(255, 255, 255, 25));

        }

        public static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(800, 800, "Pixel Logic");
            Raylib.SetWindowMinSize(300,300);
            Raylib.SetTargetFPS(60);

            Grid grid = new Grid(200, 200, GRIDSIZE);

            while (!Raylib.WindowShouldClose())
            {
                // init screen
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);
                // check mouse and keyboard input
                Input(grid);
                grid.Input();
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