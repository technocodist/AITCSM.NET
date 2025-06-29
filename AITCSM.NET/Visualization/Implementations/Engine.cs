using AITCSM.NET.Visualization.Abstractions;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;

namespace AITCSM.NET.Visualization.Implementations;

public class Engine : IEngine
{
    private readonly IWindow _window;
    private readonly GL _gl;
    private readonly IInputContext _inputContext;
    private readonly ImGuiController _imGuiController;
    private readonly IRenderer _renderer;
    private readonly ICamera _camera;
    private readonly InputHandler _inputHandler;
    private Vector2 _lastMousePosition;
    private bool _firstMove = true;

    public Engine(
        IWindow window,
        GL gl,
        IInputContext inputContext,
        ImGuiController imGuiController,
        IRenderer renderer,
        ICamera camera,
        InputHandler inputHandler)
    {
        _window = window;
        _gl = gl;
        _inputContext = inputContext;
        _imGuiController = imGuiController;
        _renderer = renderer;
        _camera = camera;
        _inputHandler = inputHandler;
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;
        _window.Resize += OnResize;
    }

    public void Run()
    {
        _window.Run();
    }

    private void OnLoad()
    {
        _inputHandler.RegisterInputEvents();
        ImGui.GetIO().FontGlobalScale = 1.5f;
        _gl.ClearColor(System.Drawing.Color.FromArgb(255, 25, 25, 50));
        _gl.Enable(EnableCap.DepthTest);
    }

    private void OnUpdate(double deltaTime)
    {
        // Only update camera and game logic here
        if (_inputHandler.PrimaryKeyboard != null)
            _camera.Update(_inputHandler.PrimaryKeyboard, (float)deltaTime);
    }

    private void OnRender(double deltaTime)
    {
        _imGuiController.Update((float)deltaTime); // Moved here for correct ImGui frame
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _renderer.Render(deltaTime);
        ImGui.Begin("Engine Debug");
        ImGui.Text($"FPS: {1.0 / deltaTime:F2}");
        ImGui.Text($"Camera Position: {_camera.Position}");
        ImGui.Text($"Cursor Mode: {_inputHandler.PrimaryMouse?.Cursor.CursorMode}");
        ImGui.Text($"Mouse Position: {_lastMousePosition}");
        ImGui.Text("Press 'Tab' to toggle cursor.");
        ImGui.End();
        _imGuiController.Render();
    }

    private void OnClose()
    {
        _imGuiController.Dispose();
    }

    private void OnResize(Silk.NET.Maths.Vector2D<int> size)
    {
        _gl.Viewport(size);
        _camera.AspectRatio = size.X / (float)size.Y;
    }

    public void KeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        // Always handle engine/game input, regardless of ImGui capture
        if (key == Key.Escape)
        {
            _window.Close();
        }
        if (key == Key.Tab)
        {
            if (_inputHandler.PrimaryMouse != null)
            {
                var cursor = _inputHandler.PrimaryMouse.Cursor;
                cursor.CursorMode = cursor.CursorMode == CursorMode.Raw
                    ? CursorMode.Normal
                    : CursorMode.Raw;
                _firstMove = true;
            }
        }
    }

    public void OnMouseMove(IMouse mouse, Vector2 position)
    {
        if (mouse.Cursor.CursorMode != CursorMode.Raw) return;
        if (_firstMove)
        {
            _lastMousePosition = position;
            _firstMove = false;
            return;
        }
        float deltaX = position.X - _lastMousePosition.X;
        float deltaY = position.Y - _lastMousePosition.Y;
        _camera.ProcessMouseMovement(deltaX, deltaY);
        _lastMousePosition = position;
    }

    public InputHandler InputHandler => _inputHandler;
}