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
    public abstract void Run();
}