using Raylib_cs;

namespace AITCSM.NET.Visualization.Abstractions;

public record struct VisualizationOptions(
    string Title,
    int Width,
    int Height,
    int FPS
);

public abstract class VisualizationBase(VisualizationOptions options)
{
    public VisualizationOptions Options { get; } = options;

    public abstract void Initialize();
    public abstract void Update();
    public abstract void Render();

    public void Run()
    {
        // Initialize window
        Raylib.InitWindow(Options.Width, Options.Height, Options.Title);
        Raylib.SetTargetFPS(Options.FPS);

        Initialize();

        while (!Raylib.WindowShouldClose())
        {
            Update();

            Render();
        }

        Raylib.CloseWindow();
    }
}