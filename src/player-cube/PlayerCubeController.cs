namespace CubeWorld;

using Godot;
using Godot.Collections;

public partial class PlayerCubeController : Node3D
{
    private bool IsMoving = false;
    private float Speed = 0.3f;
    private float Dimension = 2;

    private Camera3D CurrentCam;

    private uint LAYER2_CUBE = 1 << 1;
    private uint LAYER13_TILE = 1 << 12;

    private Vector3 PendingRotationDirection = Vector3.Zero;
    private GridCell PendingRotationTargetCell;

    [Signal]
    public delegate void GridCellPathRequestedEventHandler(Vector3 fromGlobalPosition, Vector3 toGlobalPosition, Callable mthd);

    public override void _Ready()
    {
        MeshInstance3D f = GDExtra.GetChildrenOfType<MeshInstance3D>(this, true)[0];
        Dimension = f.GetAabb().Size.X;
        this.CurrentCam = this.GetViewport().GetCamera3D();
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey @keyEvent && (@keyEvent.IsActionPressed("ui_left") || @keyEvent.IsActionPressed("ui_right") || @keyEvent.IsActionPressed("ui_up") || @keyEvent.IsActionPressed("ui_down")))
        {
            Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            if (input.Length() != 0)
            {   // ensure if X axis has value, Y axis is 0, to prevent diagonal movement 
                Vector3 direction = new Vector3(Mathf.Sign(input.X), 0, input.X == 0 ? Mathf.Sign(input.Y) : 0);
                // direction = direction.Rotated(Vector3.Up, CurrentCam.Rotation.Y);
                this.EmitSignal("GridCellPathRequested", this.GlobalPosition, this.GlobalPosition + (direction * Dimension), Callable.From((Array<GridCell> gridCellPoints) =>
                {
                    // populate values to be picked up by _PhysicsProcess for raycasting to calcuation rotation target
                    if (gridCellPoints.Count == 2)
                    {
                        this.PendingRotationDirection = direction;
                        this.PendingRotationTargetCell = gridCellPoints[1];
                    }
                    else
                    {
                        GD.Print("Cube path request was not 2 points, therefore its too long or too short! No rolling for Cubey!");
                    }
                }));
            }
            GetViewport().SetInputAsHandled();
        }

    }

    public override void _Process(double delta)
    {

    }

    private void LerpTransform(float progress, Transform3D startTransform, Vector3 pivot, Vector3 rotationAxis, float targetAngle)
    {
        this.GlobalTransform = MathR.RotateAround(startTransform, pivot, rotationAxis, Mathf.DegToRad(targetAngle * progress));
    }

    public override void _PhysicsProcess(double delta)
    {
        // Calculate angle between Tile and Cube, to use for target roll
        if (this.PendingRotationTargetCell != null && this.PendingRotationDirection != Vector3.Zero)
        {
            Vector3 positionTile = this.PendingRotationTargetCell.Node.GlobalPosition;
            GD.Print("positionTile =" + positionTile);
            PhysicsRayQueryParameters3D rayQueryTile = PhysicsRayQueryParameters3D.Create(positionTile + Vector3.Up, positionTile + Vector3.Down, LAYER13_TILE);
            Dictionary resultTile = this.GetWorld3D().DirectSpaceState.IntersectRay(rayQueryTile);
            Vector3 normalTile = Vector3.Zero;
            if (resultTile.Count != 0)
            {
                normalTile = resultTile["normal"].As<Vector3>();
            }

            Vector3 positionCube = this.GlobalPosition;
            PhysicsRayQueryParameters3D rayQueryCube = PhysicsRayQueryParameters3D.Create(positionCube + (PendingRotationDirection * 3), positionCube, LAYER2_CUBE);
            Dictionary resultCube = this.GetWorld3D().DirectSpaceState.IntersectRay(rayQueryCube);
            Vector3 normalCube = Vector3.Zero;
            if (resultCube.Count != 0)
            {
                normalCube = resultCube["normal"].As<Vector3>();
            }

            GD.Print("normalTile = " + normalTile);
            GD.Print("normalCube = " + normalCube);

            this.PendingRotationTargetCell = null;
            if (normalTile != Vector3.Zero && normalCube != Vector3.Zero)
            {
                float angle = Mathf.RadToDeg(normalCube.AngleTo(normalTile));
                GD.Print("angle = " + angle);
                // angle = 90;
                this.RotateCube(this.PendingRotationDirection, 90);
            }

        }
    }
    public void RotateCube(Vector3 direction, float targetAngle = 90)
    {
        if (IsMoving) return;
        IsMoving = true;
        Vector3 pivot = this.GlobalTransform.Origin + (Vector3.Down * 0.5f * Dimension) + (direction * 0.5f * Dimension);

        Vector3 rotationAxis = Vector3.Up.Cross(direction);
        Transform3D startTransform = this.GlobalTransform;

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
        this.PendingRotationDirection = Vector3.Zero;

    }

}
