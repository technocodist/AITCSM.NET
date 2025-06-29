using System;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Extensions.ImGui;
using ImGuiNET;

namespace AITCSM.NET;

public unsafe class Program
{
    // --- Engine Configuration ---
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private const string WindowTitle = "Gemini 2D/3D Engine with ImGui";

    // --- Core Objects ---
    private static IWindow? _window;
    private static GL? _gl;
    private static IInputContext? _inputContext;
    private static IKeyboard? _primaryKeyboard;
    private static IMouse? _primaryMouse;
    private static ImGuiController? _imGuiController;

    // --- 3D Scene Objects ---
    private static uint _cubeVao;
    private static uint _cubeVbo;
    private static uint _cubeEbo;
    private static uint _shaderProgram3D;
    private static int _modelLocation3D;
    private static int _viewLocation3D;
    private static int _projectionLocation3D;
    
    // --- Camera ---
    private static Camera? _camera;
    private static Vector2 _lastMousePosition;
    private static bool _firstMove = true;

    // Vertex data for a 3D cube. Each vertex has a position (Vector3) and a color (Vector3).
    private static readonly float[] _cubeVertices =
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

    // Element indices for the 3D cube. This tells OpenGL which vertices to combine to form triangles.
    private static readonly uint[] _cubeIndices =
    {
        0, 1, 2,  0, 2, 3,    // Back face
        4, 5, 6,  4, 6, 7,    // Front face
        8, 9, 10, 8, 10, 11,  // Left face
        12, 13, 14, 12, 14, 15, // Right face
        16, 17, 18, 16, 18, 19, // Bottom face
        20, 21, 22, 20, 22, 23  // Top face
    };

    // Simple vertex shader for 3D objects.
    private const string VertexShader3DSource = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec3 aColor;

            out vec3 fColor;

            uniform mat4 uModel;
            uniform mat4 uView;
            uniform mat4 uProjection;

            void main()
            {
                gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
                fColor = aColor;
            }
        ";

    // Simple fragment shader for 3D objects.
    private const string FragmentShader3DSource = @"
            #version 330 core
            in vec3 fColor;
            out vec4 FragColor;

            void main()
            {
                FragColor = vec4(fColor, 1.0);
            }
        ";
    
    static void Main(string[] args)
    {
        // Setup the window
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(WindowWidth, WindowHeight);
        options.Title = WindowTitle;
        _window = Window.Create(options);

        // Assign engine callbacks
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;
        _window.Resize += OnResize;

        // Run the engine
        _window.Run();
    }

    private static void OnLoad()
    {
        // --- Initialization ---
        _gl = _window!.CreateOpenGL();
        _inputContext = _window!.CreateInput();
        _imGuiController = new ImGuiController(_gl, _window, _inputContext);

        // Setup input handling
        _primaryKeyboard = _inputContext.Keyboards.FirstOrDefault();
        _primaryMouse = _inputContext.Mice.FirstOrDefault();
        if (_primaryKeyboard != null)
        {
            _primaryKeyboard.KeyDown += KeyDown;
        }
        if (_primaryMouse != null)
        {
            _primaryMouse.Cursor.CursorMode = CursorMode.Raw;
            _primaryMouse.MouseMove += OnMouseMove;
        }

        // --- 3D Object Setup ---
        _cubeVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_cubeVao);

        _cubeVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _cubeVbo);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_cubeVertices.Length * sizeof(float)), ref _cubeVertices[0], BufferUsageARB.StaticDraw);
        
        _cubeEbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _cubeEbo);
        _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(_cubeIndices.Length * sizeof(uint)), ref _cubeIndices[0], BufferUsageARB.StaticDraw);

        // --- 3D Shader Setup ---
        _shaderProgram3D = CreateShaderProgram(VertexShader3DSource, FragmentShader3DSource);
        const uint posLoc3D = 0;
        const uint colorLoc3D = 1;
        int stride3D = 6 * sizeof(float);
        _gl.EnableVertexAttribArray(posLoc3D);
        _gl.VertexAttribPointer(posLoc3D, 3, VertexAttribPointerType.Float, false, (uint)stride3D, (void*)0);
        _gl.EnableVertexAttribArray(colorLoc3D);
        _gl.VertexAttribPointer(colorLoc3D, 3, VertexAttribPointerType.Float, false, (uint)stride3D, (void*)(3 * sizeof(float)));
        
        _modelLocation3D = _gl.GetUniformLocation(_shaderProgram3D, "uModel");
        _viewLocation3D = _gl.GetUniformLocation(_shaderProgram3D, "uView");
        _projectionLocation3D = _gl.GetUniformLocation(_shaderProgram3D, "uProjection");

        _gl.BindVertexArray(0);

        // --- Camera Setup ---
        _camera = new Camera(Vector3.UnitZ * 3, WindowWidth / (float)WindowHeight);

        // --- OpenGL Global State ---
        _gl.ClearColor(Color.FromArgb(255, 25, 25, 50));
        _gl.Enable(EnableCap.DepthTest);
    }

    private static void OnUpdate(double deltaTime)
    {
        _imGuiController!.Update((float)deltaTime);
        _camera?.Update(_primaryKeyboard!, (float)deltaTime);
    }

    private static void OnRender(double deltaTime)
    {
        _gl!.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Render3DScene(deltaTime);
        // --- Render ImGui UI ---
        ImGui.Begin("Engine Debug");
        ImGui.Text($"FPS: {1.0 / deltaTime:F2}");
        ImGui.Text($"Camera Position: {_camera?.Position}");
        ImGui.Text("Press 'Tab' to toggle cursor.");
        ImGui.End();
        _imGuiController!.Render();
    }

    private static void Render3DScene(double deltaTime)
    {
        _gl!.Enable(EnableCap.DepthTest);
        _gl.UseProgram(_shaderProgram3D);
        _gl.BindVertexArray(_cubeVao);
        var model = Matrix4x4.CreateRotationY((float)Math.IEEERemainder(_window!.Time * 25.0, 360.0) * ((float)Math.PI / 180f));
        var view = _camera!.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix();
        _gl.UniformMatrix4(_modelLocation3D, 1, false, in model.M11);
        _gl.UniformMatrix4(_viewLocation3D, 1, false, in view.M11);
        _gl.UniformMatrix4(_projectionLocation3D, 1, false, in projection.M11);
        _gl.DrawElements(PrimitiveType.Triangles, (uint)_cubeIndices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    private static void OnClose()
    {
        _imGuiController?.Dispose();
        _gl?.DeleteBuffer(_cubeVbo);
        _gl?.DeleteBuffer(_cubeEbo);
        _gl?.DeleteVertexArray(_cubeVao);
        _gl?.DeleteProgram(_shaderProgram3D);
    }

    private static void OnResize(Silk.NET.Maths.Vector2D<int> size)
    {
        _gl?.Viewport(size);
        if (_camera != null)
        {
            _camera.AspectRatio = size.X / (float)size.Y;
        }
    }
    
    private static void OnMouseMove(IMouse mouse, Vector2 position)
    {
        if (mouse.Cursor.CursorMode != CursorMode.Raw) return;
        if (_firstMove)
        {
            _lastMousePosition = position;
            _firstMove = false;
        }
        else
        {
            float deltaX = position.X - _lastMousePosition.X;
            float deltaY = position.Y - _lastMousePosition.Y;
            _lastMousePosition = position;
            _camera?.ProcessMouseMovement(deltaX, deltaY);
        }
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape)
        {
            _window?.Close();
        }
        if (key == Key.Tab)
        {
            if (_primaryMouse != null)
            {
                _primaryMouse.Cursor.CursorMode = _primaryMouse.Cursor.CursorMode == CursorMode.Raw 
                    ? CursorMode.Normal 
                    : CursorMode.Raw;
                _firstMove = true; // Reset last mouse position on mode change
            }
        }
    }

    private static uint CreateShaderProgram(string vertexSrc, string fragmentSrc)
    {
        uint vertexShader = _gl!.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexSrc);
        _gl.CompileShader(vertexShader);
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
        
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentSrc);
        _gl.CompileShader(fragmentShader);
        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));

        uint program = _gl.CreateProgram();
        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);
        _gl.LinkProgram(program);
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int pStatus);
        if (pStatus != (int)GLEnum.True)
            throw new Exception("Shader program failed to link: " + _gl.GetProgramInfoLog(program));

        _gl.DetachShader(program, vertexShader);
        _gl.DetachShader(program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        return program;
    }
}

// A simple Camera class for navigating the 3D scene.
public class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; }
    private float _pitch;
    private float _yaw = -90f;
    private float _fov = 45f;
    
    public float AspectRatio { get; set; }
    public float MoveSpeed { get; set; } = 2.5f;
    public float MouseSensitivity { get; set; } = 0.1f;

    public Camera(Vector3 position, float aspectRatio)
    {
        Position = position;
        AspectRatio = aspectRatio;
        UpdateVectors();
    }

    public Matrix4x4 GetViewMatrix() => Matrix4x4.CreateLookAt(Position, Position + Front, Up);

    public Matrix4x4 GetProjectionMatrix() => Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov), AspectRatio, 0.1f, 100.0f);

    public void Update(IKeyboard keyboard, float deltaTime)
    {
        if (keyboard == null) return;
        float speed = MoveSpeed * deltaTime;
        if (keyboard.IsKeyPressed(Key.W)) Position += Front * speed;
        if (keyboard.IsKeyPressed(Key.S)) Position -= Front * speed;
        if (keyboard.IsKeyPressed(Key.A)) Position -= Right * speed;
        if (keyboard.IsKeyPressed(Key.D)) Position += Right * speed;
        if (keyboard.IsKeyPressed(Key.Space)) Position += Up * speed;
        if (keyboard.IsKeyPressed(Key.ShiftLeft)) Position -= Up * speed;
    }

    public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
    {
        xOffset *= MouseSensitivity;
        yOffset *= MouseSensitivity;

        _yaw += xOffset;
        _pitch -= yOffset;

        if (constrainPitch)
        {
            _pitch = Math.Clamp(_pitch, -89.0f, 89.0f);
        }
        UpdateVectors();
    }

    private void UpdateVectors()
    {
        Vector3 front;
        front.X = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(_yaw));
        front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(_pitch));
        front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(_yaw));
        Front = Vector3.Normalize(front);

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }
}

// Simple static class for math helpers
public static class MathHelper
{
    public static float DegreesToRadians(float degrees)
    {
        return degrees * (float)Math.PI / 180.0f;
    }
}
