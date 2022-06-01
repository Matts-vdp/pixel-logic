﻿using Raylib_cs;
using System.Numerics;
using Game.Components;

namespace Game
{
    class FileReader : IFile
    {
        public string ReadAllText(string filename)
        {
            return File.ReadAllText(filename);
        }
        public void WriteAllTextAsync(string filename, string txt)
        {
            File.WriteAllTextAsync(filename, txt);
        }
    }

    // contains the main game loop
    class PixelLogic {
        // main game loop
        public static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(800, 800, "Pixel Logic");
            Raylib.SetWindowMinSize(300, 300);
            Raylib.SetTargetFPS(60);
            Simulation gm = new Simulation(new FileReader());

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