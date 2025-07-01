using Raylib_cs;
using System.Numerics;

namespace AITCSM.NET.Visualization
{
    public class VisualizationBase
    {
        public static void Run()
        {
            // Initialize window
            Raylib.InitWindow(800, 800, "3D Bouncing Ball");
            Raylib.SetTargetFPS(60);

            // Set up camera
            Camera3D camera = new()
            {
                Position = new Vector3(0.0f, 10.0f, 20.0f),
                Target = new Vector3(0.0f, 0.0f, 0.0f),
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                FovY = 45.0f,
                Projection = CameraProjection.Perspective
            };

            // Ball properties
            Vector3 ballPosition = new(0.0f, 5.0f, 0.0f);
            Vector3 ballVelocity = new(2.0f, 0.0f, 2.0f);
            float ballRadius = 1.0f;
            Color ballColor = Color.Red;

            // Boundaries for the box
            Vector3 boxMin = new(-10.0f, 0.0f, -10.0f);
            Vector3 boxMax = new(10.0f, 10.0f, 10.0f);

            // Set up lighting
            Vector3 lightPosition = new(5.0f, 5.0f, 5.0f);

            while (!Raylib.WindowShouldClose())
            {
                // Update camera (orbit controls)
                Raylib.UpdateCamera(ref camera, CameraMode.FirstPerson);

                // Update ball position
                ballPosition += ballVelocity * Raylib.GetFrameTime();

                // Check collisions with box boundaries
                if (ballPosition.X + ballRadius > boxMax.X || ballPosition.X - ballRadius < boxMin.X)
                {
                    ballVelocity.X = -ballVelocity.X;
                    ballPosition.X = Math.Clamp(ballPosition.X, boxMin.X + ballRadius, boxMax.X - ballRadius);
                }
                if (ballPosition.Y + ballRadius > boxMax.Y || ballPosition.Y - ballRadius < boxMin.Y)
                {
                    ballVelocity.Y = -ballVelocity.Y;
                    ballPosition.Y = Math.Clamp(ballPosition.Y, boxMin.Y + ballRadius, boxMax.Y - ballRadius);
                }
                if (ballPosition.Z + ballRadius > boxMax.Z || ballPosition.Z - ballRadius < boxMin.Z)
                {
                    ballVelocity.Z = -ballVelocity.Z;
                    ballPosition.Z = Math.Clamp(ballPosition.Z, boxMin.Z + ballRadius, boxMax.Z - ballRadius);
                }

                // Apply gravity
                ballVelocity.Y -= 9.81f * Raylib.GetFrameTime();

                // Begin drawing
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.RayWhite);

                Raylib.BeginMode3D(camera);

                // Draw the ball
                Raylib.DrawSphere(ballPosition, ballRadius, ballColor);

                // Draw the box boundaries (wireframe)
                Raylib.DrawCubeWires(new Vector3(0.0f, 5.0f, 0.0f), boxMax.X * 2, boxMax.Y, boxMax.Z * 2, Color.Black);

                // Draw a simple ground plane
                Raylib.DrawPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector2(20.0f, 20.0f), Color.Gray);

                // Draw light position
                Raylib.DrawSphere(lightPosition, 0.2f, Color.Yellow);

                Raylib.EndMode3D();

                // Draw FPS
                Raylib.DrawFPS(10, 10);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}