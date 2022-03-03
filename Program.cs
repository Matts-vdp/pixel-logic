using Raylib_cs;
using System.Numerics;
namespace game {
    static class Game {

        enum blocks {
            WIRE,
            POWER
        }

        static int GRIDSIZE = 32;
        static int selected = 1;
        static int xoff = 0;
        static int yoff = 0;

        public static void Input(Grid grid){
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)){
                    Vector2 pos = Raylib.GetMousePosition();
                    grid.add(pos, (types)selected, GRIDSIZE);
                }
                if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)){
                    Vector2 pos = Raylib.GetMousePosition();
                    grid.del(pos, GRIDSIZE);
                }
                selected -= (int)Raylib.GetMouseWheelMove();
                int max = Enum.GetValues(typeof(types)).Cast<int>().Max();
                if (selected > max){
                    selected = max;
                }
                if (selected < 1) {selected = 1;}

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

        public static void drawUI(){
            String[] items = {"Wire", "And", "Or", "Not", "Out", "In", "Battery", "Clock", "Flip Flop", "Button"};
            for (int i=0; i<items.Length; i++) {
                bool sel = (i+1) == selected;
                Raylib.DrawText(items[i], 20,20*(i+1),20,sel? Color.WHITE: Color.GRAY);
            }
        }

        public static void Main() {
            Raylib.InitWindow(800, 800, "Test");
            Raylib.SetTargetFPS(60);

            Grid grid = new Grid(200,200,GRIDSIZE);

            while (!Raylib.WindowShouldClose()) {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);
                
                Input(grid);

                grid.update();
                grid.draw(GRIDSIZE, xoff, yoff);
                drawUI();

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