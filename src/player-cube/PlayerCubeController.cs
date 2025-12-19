namespace CubeWorld;

using Godot;
using Godot.Collections;

public partial class PlayerCubeController : Node3D
{
    private bool IsMoving = false;
    private float Speed = 0.3f;
    private float Dimension = 2;

    private Camera3D currentCam;

    [Signal]
    public delegate void TileRequestedAtGobalPositionEventHandler(Vector3 globalPosition, Callable mthd);

    public override void _Ready()
    {
        MeshInstance3D f = GDExtra.GetChildrenOfType<MeshInstance3D>(this, true)[0];
        Dimension = f.GetAabb().Size.X;
        this.currentCam = this.GetViewport().GetCamera3D();
    }

    public override void _Process(double delta)
    {
        Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (input.Length() != 0)
        {   // ensure if X axis has value, Y axis is 0, to prevent diagonal movement 
            Vector3 direction = new Vector3(Mathf.Sign(input.X), 0, input.X == 0 ? Mathf.Sign(input.Y) : 0);
            // direction = direction.Rotated(Vector3.Up, currentCam.Rotation.Y);
            this.EmitSignal("TileRequestedAtGobalPosition", this.GlobalPosition + (direction * 0.5f * Dimension), Callable.From((GridCell targetCell) => this.RotateCube(direction, targetCell)));
        }

    }

    private void LerpTransform(float progress, Transform3D startTransform, Vector3 pivot, Vector3 rotationAxis, float targetAngle)
    {
        this.GlobalTransform = MathR.RotateAround(startTransform, pivot, rotationAxis, Mathf.DegToRad(targetAngle * progress));
    }
    public void RotateCube(Vector3 direction, GridCell targetCell)
    {
        if (IsMoving || targetCell == null) return;
        IsMoving = true;
        Vector3 pivot = this.GlobalTransform.Origin + (Vector3.Down * 0.5f * Dimension) + (direction * 0.5f * Dimension);

        Vector3 rotationAxis = Vector3.Up.Cross(direction);
        Transform3D startTransform = this.GlobalTransform;

        float targetAngle = 90;
        // Array<Node> targetCellStaticBody = targetCell.Node.FindChildren("*", "StaticBody3D");
        // foreach (StaticBody3D staticBody in targetCellStaticBody)
        // {
        //     if (staticBody.IsInGroup("grid-cell__floor"))
        //     {
        //         GD.Print("DOT" + direction.Dot(staticBody.GlobalRotationDegrees));
        //         GD.Print("AngleTo" + Mathf.RadToDeg(direction.AngleTo(staticBody.GlobalRotation)));
        //         targetAngle = direction.Dot(staticBody.GlobalRotationDegrees);
        //     }
        // }

        Tween tween = this.CreateTween();
        tween.TweenMethod(Callable.From((float progress) => this.LerpTransform(progress, startTransform, pivot, rotationAxis, targetAngle)), 0.0, 1.0, Speed);
        tween.TweenCallback(Callable.From(() => IsMoving = false));
        IsMoving = true;

    }
}
