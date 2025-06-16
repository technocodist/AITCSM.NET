using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace AITCSM.NET.Implementations.Visualization;

public class PrimitiveRenderer2D
{
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _shader;
    private readonly List<float> _vertexBuffer = new();

    private readonly int _positionLocation = 0;
    private readonly int _colorLocation = 1;

    public PrimitiveRenderer2D()
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 1024 * sizeof(float), nint.Zero, BufferUsageHint.DynamicDraw);

        GL.EnableVertexAttribArray(_positionLocation);
        GL.VertexAttribPointer(_positionLocation, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

        GL.EnableVertexAttribArray(_colorLocation);
        GL.VertexAttribPointer(_colorLocation, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float), 2 * sizeof(float));

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);

        _shader = CompileShader();
    }

    public void Begin()
    {
        _vertexBuffer.Clear();
    }

    public void End()
    {
        GL.UseProgram(_shader);
        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexBuffer.Count * sizeof(float), _vertexBuffer.ToArray(), BufferUsageHint.DynamicDraw);

        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexBuffer.Count / 6);

        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public void DrawRectangle(Vector2 center, Vector2 size, Vector4 color)
    {
        Vector2 half = size / 2f;

        Vector2 tl = center + new Vector2(-half.X, half.Y);
        Vector2 tr = center + new Vector2(half.X, half.Y);
        Vector2 br = center + new Vector2(half.X, -half.Y);
        Vector2 bl = center + new Vector2(-half.X, -half.Y);

        AddQuad(tl, tr, br, bl, color);
    }

    public void DrawCircle(Vector2 center, float radius, Vector4 color, int segments = 32)
    {
        for (int i = 0; i < segments; i++)
        {
            float a0 = MathF.PI * 2f * i / segments;
            float a1 = MathF.PI * 2f * (i + 1) / segments;

            Vector2 p0 = center;
            Vector2 p1 = center + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * radius;
            Vector2 p2 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

            AddTriangle(p0, p1, p2, color);
        }
    }

    private void AddQuad(Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl, Vector4 color)
    {
        AddTriangle(tl, tr, br, color);
        AddTriangle(br, bl, tl, color);
    }

    private void AddTriangle(Vector2 a, Vector2 b, Vector2 c, Vector4 color)
    {
        AddVertex(a, color);
        AddVertex(b, color);
        AddVertex(c, color);
    }

    private void AddVertex(Vector2 position, Vector4 color)
    {
        _vertexBuffer.Add(position.X);
        _vertexBuffer.Add(position.Y);
        _vertexBuffer.Add(color.X);
        _vertexBuffer.Add(color.Y);
        _vertexBuffer.Add(color.Z);
        _vertexBuffer.Add(color.W);
    }

    private int CompileShader()
    {
        const string vertexShader = @"
            #version 330 core
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec4 aColor;
            out vec4 vColor;
            void main()
            {
                gl_Position = vec4(aPosition, 0.0, 1.0);
                vColor = aColor;
            }";

        const string fragmentShader = @"
            #version 330 core
            in vec4 vColor;
            out vec4 FragColor;
            void main()
            {
                FragColor = vColor;
            }";

        int vs = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vs, vertexShader);
        GL.CompileShader(vs);
        GL.GetShader(vs, ShaderParameter.CompileStatus, out int vsStatus);
        if (vsStatus != (int)All.True)
            throw new Exception("Vertex Shader Error: " + GL.GetShaderInfoLog(vs));

        int fs = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fs, fragmentShader);
        GL.CompileShader(fs);
        GL.GetShader(fs, ShaderParameter.CompileStatus, out int fsStatus);
        if (fsStatus != (int)All.True)
            throw new Exception("Fragment Shader Error: " + GL.GetShaderInfoLog(fs));

        int program = GL.CreateProgram();
        GL.AttachShader(program, vs);
        GL.AttachShader(program, fs);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus != (int)All.True)
            throw new Exception("Program Link Error: " + GL.GetProgramInfoLog(program));

        GL.DeleteShader(vs);
        GL.DeleteShader(fs);

        return program;
    }
}