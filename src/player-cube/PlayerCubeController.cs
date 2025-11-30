namespace CubeWorld;

using Godot;


public partial class PlayerCubeController : Node3D
{
    private bool IsMoving = false;
    private float Speed = 0.3f;
    private float Dimension = 2;

    private Camera3D currentCam;

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
        {
            Vector3 direction = new Vector3(Mathf.Sign(input.X), 0, input.X == 0 ? Mathf.Sign(input.Y) : 0);
            // direction = direction.Rotated(Vector3.Up, currentCam.Rotation.Y);

            this.RotateCube(direction, Speed);
        }

    }

    private void LerpTransform(float progress, Transform3D startTransform, Vector3 rotationCenter, Vector3 rotationAxis)
    {

        this.GlobalTransform = MathR.RotateAround(startTransform, rotationCenter, rotationAxis, Mathf.DegToRad(90 * progress));

    }
    public void RotateCube(Vector3 direction, float speed)
    {
        if (IsMoving) return;
        IsMoving = true;
        Vector3 rotationCenter = this.GlobalTransform.Origin + (Vector3.Down * 0.5f * Dimension) + (direction * 0.5f * Dimension);

        Vector3 rotationAxis = Vector3.Up.Cross(direction);
        Transform3D startTransform = this.GlobalTransform;

        Tween tween = this.CreateTween();
        tween.TweenMethod(Callable.From((float progress) => this.LerpTransform(progress, startTransform, rotationCenter, rotationAxis)), 0.0, 1.0, speed);
        tween.TweenCallback(Callable.From(() =>
        {
            IsMoving = false;
        }));
        IsMoving = true;

    }
}
