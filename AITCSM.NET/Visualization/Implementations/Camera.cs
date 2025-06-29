using System.Numerics;
using AITCSM.NET.Visualization.Abstractions;

namespace AITCSM.NET.Visualization.Implementations;

public class Camera : ICamera
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

    public void Update(object? keyboard, float deltaTime)
    {
        if (keyboard is not Silk.NET.Input.IKeyboard k) return;
        float speed = MoveSpeed * deltaTime;
        if (k.IsKeyPressed(Silk.NET.Input.Key.W)) Position += Front * speed;
        if (k.IsKeyPressed(Silk.NET.Input.Key.S)) Position -= Front * speed;
        if (k.IsKeyPressed(Silk.NET.Input.Key.A)) Position -= Right * speed;
        if (k.IsKeyPressed(Silk.NET.Input.Key.D)) Position += Right * speed;
        if (k.IsKeyPressed(Silk.NET.Input.Key.Space)) Position += Up * speed;
        if (k.IsKeyPressed(Silk.NET.Input.Key.ShiftLeft)) Position -= Up * speed;
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

public static class MathHelper
{
    public static float DegreesToRadians(float degrees)
    {
        return degrees * (float)Math.PI / 180.0f;
    }
}
