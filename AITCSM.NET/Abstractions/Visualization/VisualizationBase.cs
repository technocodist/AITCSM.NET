using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace AITCSM.NET.Implementations.Visualization;

public abstract class VisualizationBase
{
    protected GameWindow Window;

    protected VisualizationBase(GameWindow window)
    {
        Window = window;
    }

    public virtual void OnLoad() { }
    public virtual void OnUpdateFrame(FrameEventArgs args) { }
    public virtual void OnRenderFrame(FrameEventArgs args) { }
    public virtual void OnResize(ResizeEventArgs e) { }
}
