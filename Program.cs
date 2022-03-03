using Raylib_cs;
using System.Numerics;
namespace game {
    static class Game {

        enum blocks {
            WIRE,
            POWER
        }

        static int GRIDSIZE = 32;
        static types selected = types.WIRE;
        static int xoff = 0;
        static int yoff = 0;

        public static void Input(Grid grid){
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)){
                    Vector2 pos = Raylib.GetMousePosition();
                    grid.add(pos, selected, GRIDSIZE);
                }
                if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)){
                    Vector2 pos = Raylib.GetMousePosition();
                    grid.del(pos, GRIDSIZE);
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE)){
                    selected = types.WIRE;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_TWO)){
                    selected = types.BATTERY;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_THREE)){
                    selected = types.IN;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_FOUR)){
                    selected = types.OUT;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_FIVE)){
                    selected = types.AND;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SIX)){
                    selected = types.CLK;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SEVEN)){
                    selected = types.OR;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_EIGHT)){
                    selected = types.NOT;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_NINE)){
                    selected = types.CLK;
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_ADD)){
                    GRIDSIZE += 1;
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_SUBTRACT)){
                    GRIDSIZE -= 1;
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)){
                    yoff += 4;
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)){
                    yoff -= 4;
                    if (yoff<0){yoff = 0;}
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)){
                    xoff += 4;
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)){
                    xoff -= 4;
                    if (xoff<0){xoff = 0;}
                }
        }

        public static void Main() {
            Raylib.InitWindow(800, 400, "Test");
            Raylib.SetTargetFPS(60);

            Grid grid = new Grid(200,200,GRIDSIZE);

            while (!Raylib.WindowShouldClose()) {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);
                
                Input(grid);

                grid.update();
                grid.draw(GRIDSIZE, xoff, yoff);

                Vector2 mpos = Raylib.GetMousePosition();
                int x = grid.toGrid(mpos.X, GRIDSIZE);
                int y = grid.toGrid(mpos.Y, GRIDSIZE);
                Raylib.DrawRectangle(x*GRIDSIZE,y*GRIDSIZE, GRIDSIZE, GRIDSIZE, new Color(255,255,255,25));

                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}