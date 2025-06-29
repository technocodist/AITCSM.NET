using AITCSM.NET.Visualization.Implementations;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Numerics;

namespace AITCSM.NET;

public static class Program
{
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private const string WindowTitle = "Techno 2D/3D Engine with ImGui";

    private static readonly float[] CubeVertices =
    {
        // Position          // Color
        -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f, // Back face
         0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.0f,

        -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 0.0f, // Front face
         0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 0.0f,

        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f, // Left face
        -0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 1.0f,

         0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.0f, // Right face
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f,
         0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 1.0f, 0.0f,

        -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 1.0f, // Bottom face
         0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 1.0f,

        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 1.0f, // Top face
         0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 1.0f
    };

    private static readonly uint[] CubeIndices =
    {
        0, 1, 2,  0, 2, 3,    // Back face
        4, 5, 6,  4, 6, 7,    // Front face
        8, 9, 10, 8, 10, 11,  // Left face
        12, 13, 14, 12, 14, 15, // Right face
        16, 17, 18, 16, 18, 19, // Bottom face
        20, 21, 22, 20, 22, 23  // Top face
    };

    public static void Main(string[] args)
    {
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(WindowWidth, WindowHeight);
        options.Title = WindowTitle;
        options.UpdatesPerSecond = 60;
        options.FramesPerSecond = 60;
        var window = Window.Create(options);
        window.Load += () =>
        {
            var gl = window.CreateOpenGL();
            var inputContext = window.CreateInput();
            var imGuiController = new ImGuiController(gl, window, inputContext);
            var camera = new Camera(Vector3.UnitZ * 3, WindowWidth / (float)WindowHeight);
            var renderer = new Renderer(gl, CubeVertices, CubeIndices, camera, () => window.Time);
            // Create InputHandler first
            InputHandler? inputHandler = null;
            Engine? engine = null;
            inputHandler = new InputHandler(inputContext, window, (k, key, arg3) => engine?.KeyDown(k, key, arg3), (m, pos) => engine?.OnMouseMove(m, pos));
            engine = new Engine(window, gl, inputContext, imGuiController, renderer, camera, inputHandler);
            // Register input events
            inputHandler.RegisterInputEvents();
        };
        window.Run();
    }
}
