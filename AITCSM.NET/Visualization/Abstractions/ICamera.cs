using System.Numerics;

namespace AITCSM.NET.Visualization.Abstractions;

public interface ICamera
{
    Vector3 Position { get; set; }
    float AspectRatio { get; set; }
    void Update(object? keyboard, float deltaTime);
    void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true);
    Matrix4x4 GetViewMatrix();
    Matrix4x4 GetProjectionMatrix();
}
