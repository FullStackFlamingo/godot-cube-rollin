using Godot;
using System;

namespace CubeWorld
{
    public partial class CubeController : Node3D
    {
        [Export]
        private Camera3D _camera;
        private bool IsMoving = false;
        private float Speed = 0.3f;
        private float Dimension = 2;

        public override void _Ready()
        {
            // MeshInstance3D f = (MeshInstance3D)FindChild("MeshInstance3D", true, false);
            // Dimension = f.GetAabb().Size.X;
        }

        public override void _Process(double delta)
        {
            Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            if (input.Length() != 0)
            {
                Vector3 direction = new Vector3(Mathf.Sign(input.X), 0, input.X == 0 ? Mathf.Sign(input.Y) : 0)/* .Rotated(Vector3.Up, _camera.Rotation.Y) */;
                RotateCube(direction, Speed);
            }

        }

        private void LerpTransform(float progress, Transform3D startTransform, Vector3 rotationCenter, Vector3 rotationAxis)
        {

            GlobalTransform = MathR.RotateAround(startTransform, rotationCenter, rotationAxis, Mathf.DegToRad(90 * progress));

            _camera.Position = new Vector3(0 + GlobalTransform.Origin.X, 10, 10 + GlobalTransform.Origin.Z);
        }
        public void RotateCube(Vector3 direction, float speed)
        {
            if (IsMoving) return;
            IsMoving = true;
            Vector3 rotationCenter = GlobalTransform.Origin + (Vector3.Down * 0.5f * Dimension) + (direction * 0.5f * Dimension);

            Vector3 rotationAxis = Vector3.Up.Cross(direction);
            Transform3D startTransform = GlobalTransform;

            Tween tween = CreateTween();
            tween.TweenMethod(Callable.From((float progress) => LerpTransform(progress, startTransform, rotationCenter, rotationAxis)), 0.0, 1.0, speed);
            tween.TweenCallback(Callable.From(() =>
            {
                IsMoving = false;
            }));
            IsMoving = true;

        }
    }
}
