using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace AITCSM.NET.Implementations.Visualization;

public class VisualizationWindow : GameWindow
{
    public VisualizationBase Visualization { get; private set; }

    public VisualizationWindow(VisualizationBase visualization, GameWindowSettings gws, NativeWindowSettings nws)
        : base(gws, nws)
    {
        Visualization = visualization;
    }

    protected override void OnLoad() => Visualization.OnLoad();
    protected override void OnUpdateFrame(FrameEventArgs args) => Visualization.OnUpdateFrame(args);

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        Visualization.OnRenderFrame(args);
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e) => Visualization.OnResize(e);
}
