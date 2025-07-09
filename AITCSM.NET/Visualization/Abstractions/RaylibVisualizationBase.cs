using Raylib_cs;
using rlImGui_cs;

namespace AITCSM.NET.Visualization.Abstractions;

public abstract class RaylibVisualizationBase(VisualizationOptions options) : VisualizationBase(options)
{
    public override void Run()
    {
        // Initialize window
        Raylib.InitWindow(Options.Width, Options.Height, Options.Title);
        Raylib.SetTargetFPS(Options.FPS);
        rlImGui.Setup(true);

        Initialize();

        while (!Raylib.WindowShouldClose())
        {
            rlImGui.Begin();
            Update();

            Render();
            rlImGui.End();
        }

        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}