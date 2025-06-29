using AITCSM.NET.Visualization.Abstractions;
using Silk.NET.OpenGL;
using System.Numerics;

namespace AITCSM.NET.Visualization.Implementations;

public unsafe class Renderer : IRenderer
{
    private readonly GL _gl;
    private readonly uint _vao, _vbo, _ebo, _shaderProgram;
    private readonly int _modelLoc, _viewLoc, _projLoc;
    private readonly float[] _vertices;
    private readonly uint[] _indices;
    private readonly ICamera _camera;
    private readonly Func<double> _getTime;

    public Renderer(GL gl, float[] vertices, uint[] indices, ICamera camera, Func<double> getTime)
    {
        _gl = gl;
        _vertices = vertices;
        _indices = indices;
        _camera = camera;
        _getTime = getTime;
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(float)), ref _vertices[0], BufferUsageARB.StaticDraw);
        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(_indices.Length * sizeof(uint)), ref _indices[0], BufferUsageARB.StaticDraw);
        _shaderProgram = CreateShaderProgram(gl, VertexShaderSource, FragmentShaderSource);
        const uint posLoc = 0;
        const uint colorLoc = 1;
        int stride = 6 * sizeof(float);
        _gl.EnableVertexAttribArray(posLoc);
        _gl.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
        _gl.EnableVertexAttribArray(colorLoc);
        _gl.VertexAttribPointer(colorLoc, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)(3 * sizeof(float)));
        _modelLoc = _gl.GetUniformLocation(_shaderProgram, "uModel");
        _viewLoc = _gl.GetUniformLocation(_shaderProgram, "uView");
        _projLoc = _gl.GetUniformLocation(_shaderProgram, "uProjection");
        _gl.BindVertexArray(0);
    }

    public void Render(double deltaTime)
    {
        _gl.Enable(EnableCap.DepthTest);
        _gl.UseProgram(_shaderProgram);
        _gl.BindVertexArray(_vao);
        var model = Matrix4x4.CreateRotationY((float)System.Math.IEEERemainder(_getTime() * 25.0, 360.0) * ((float)System.Math.PI / 180f));
        var view = _camera.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix();
        _gl.UniformMatrix4(_modelLoc, 1, false, in model.M11);
        _gl.UniformMatrix4(_viewLoc, 1, false, in view.M11);
        _gl.UniformMatrix4(_projLoc, 1, false, in projection.M11);
        _gl.DrawElements(PrimitiveType.Triangles, (uint)_indices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    private static uint CreateShaderProgram(GL gl, string vertexSrc, string fragmentSrc)
    {
        uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, vertexSrc);
        gl.CompileShader(vertexShader);
        gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + gl.GetShaderInfoLog(vertexShader));
        uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, fragmentSrc);
        gl.CompileShader(fragmentShader);
        gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + gl.GetShaderInfoLog(fragmentShader));
        uint program = gl.CreateProgram();
        gl.AttachShader(program, vertexShader);
        gl.AttachShader(program, fragmentShader);
        gl.LinkProgram(program);
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int pStatus);
        if (pStatus != (int)GLEnum.True)
            throw new Exception("Shader program failed to link: " + gl.GetProgramInfoLog(program));
        gl.DetachShader(program, vertexShader);
        gl.DetachShader(program, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
        return program;
    }

    private const string VertexShaderSource = @"#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;
out vec3 fColor;
uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
void main() {
    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
    fColor = aColor;
}";
    private const string FragmentShaderSource = @"#version 330 core
in vec3 fColor;
out vec4 FragColor;
void main() {
    FragColor = vec4(fColor, 1.0);
}";
}
