using Raylib_cs;
using System.Numerics;

namespace Game
{
    // contains the main game loop
    class PixelLogic {
        // main game loop
        public static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(800, 800, "Pixel Logic");
            Raylib.SetWindowMinSize(300, 300);
            Raylib.SetTargetFPS(60);
            Simulation gm = new Simulation();

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
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}