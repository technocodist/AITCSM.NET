using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace AITCSM.NET.Implementations.Visualization;

public class BouncingParticleSimulation(GameWindow window) : VisualizationBase(window)
{
    private Vector2 position = new(0, 0.8f);
    private Vector2 velocity = new(0, 0);
    private const float Gravity = -9.8f;
    private const float Radius = 0.05f;

    private PrimitiveRenderer2D _renderer = null!;

    public override void OnLoad()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
        _renderer = new PrimitiveRenderer2D();
    }

    public override void OnUpdateFrame(FrameEventArgs args)
    {
        velocity.Y += Gravity * (float)args.Time;
        position += velocity * (float)args.Time;

        if (position.Y - Radius < -1f)
        {
            position.Y = -1f + Radius;
            velocity.Y *= -0.8f;
        }
    }

    public override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        _renderer.Begin();
        _renderer.DrawCircle(position, Radius, new Vector4(1f, 0.2f, 0.2f, 1f));
        _renderer.End();

        Window.SwapBuffers();
    }

    public override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, int.Max(e.Width, e.Height), int.Max(e.Width, e.Height));
    }

    public static void Run()
    {
        var gws = new GameWindowSettings()
        {
            UpdateFrequency = 60.0
        };

        var nws = new NativeWindowSettings()
        {
            ClientSize = new OpenTK.Mathematics.Vector2i(800, y: 800),
            Title = "Physics Visualization"
        };

        VisualizationWindow window = null!;
        var simulation = new BouncingParticleSimulation(window);
        window = new VisualizationWindow(simulation, gws, nws);

        simulation = new BouncingParticleSimulation(window);
        window = new VisualizationWindow(simulation, gws, nws);

        window.Run();
    }
}