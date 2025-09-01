using Godot;
using System;

public partial class CubeController : Node3D
{
    [Export]
    private Camera3D _camera;
    private bool IsMoving = false;
    private int Speed = 1;
    private int MagicDimension = 2;
    private float DiagonalFaceLength = Mathf.Sqrt(2 * 2);

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (input.Length() != 0)
        {
            Vector3 direction = new Vector3(input.X, 0, input.Y).Rotated(Vector3.Up, _camera.Rotation.Y);
            RotateCube(direction, Speed);
        }

    }

    // Inverted catenary
    private void RollHeight(float originalY, float progress)
    {
        // from 0 to 1 to 0
        float xx = 2 * (0.5f - Mathf.Abs(progress - 0.5f));
        // float h = DiagonalFaceLength / 2.0f * Mathf.Cosh(1) - DiagonalFaceLength / 2.0f * Mathf.Cosh(xx / (DiagonalFaceLength / 2.0f));

        var newTransform = Transform;
        newTransform.Origin.Y = originalY + (0.1f * DiagonalFaceLength * xx);
        Transform = newTransform;
    }
    private void LerpBasis(float progress, Basis f, Basis t)
    {
        var newTransform = GlobalTransform;
        newTransform.Basis = f.Slerp(t, progress);
        GlobalTransform = newTransform;
    }
    public void RotateCube(Vector3 direction, float speed)
    {
        if (IsMoving) return;
        IsMoving = true;
        // rotationBasis.Rotated(new Vector3(1, 0, 0), Mathf.DegToRad(90));

        Vector3 rollDestination = Position + (direction * MagicDimension);

        //  The final resting basis after the roll
        Basis rollBasis = GlobalTransform.Basis;
        rollBasis = rollBasis.Rotated(Vector3.Forward, Mathf.DegToRad(90) * Mathf.Sign(direction.X));
        rollBasis = rollBasis.Rotated(Vector3.Left, Mathf.DegToRad(90) * -Mathf.Sign(direction.Z));
        // rollBasis = rollBasis * GlobalTransform.Basis;

        float originalY = GlobalTransform.Origin.Y;
        Console.WriteLine("originalY" + originalY);

        Tween tween = CreateTween();
        tween.TweenProperty(this, "position", rollDestination, speed);
        tween.Parallel().TweenMethod(Callable.From((float progress) => LerpBasis(progress, GlobalTransform.Basis, rollBasis)), 0.0, 1.0, speed);
        tween.Parallel().TweenMethod(Callable.From((float progress) => RollHeight(originalY, progress)), 0.0, 1.0, speed);
        tween.TweenCallback(
            Callable.From(() =>
            {
                IsMoving = false;
                Console.WriteLine(GlobalTransform.Origin.ToString());
            }
            )
        );
    }
}
