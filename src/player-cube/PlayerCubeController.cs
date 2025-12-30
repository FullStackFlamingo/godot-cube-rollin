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
                        GD.Print("Cube path request was not 2 points (It was " + gridCellPoints.Count + " points), therefore its too long or too short! No rolling for Cubey!");
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

            // normal of target tile, to calculate angle 
            Vector3 positionTargetTile = this.PendingRotationTargetCell.GlobalPosition;
            PhysicsRayQueryParameters3D rayQueryTargetTile = PhysicsRayQueryParameters3D.Create(positionTargetTile + Vector3.Up, positionTargetTile + Vector3.Down, LAYER13_TILE);
            Dictionary resultTargetTile = this.GetWorld3D().DirectSpaceState.IntersectRay(rayQueryTargetTile);
            Vector3 normalTargetTile = Vector3.Zero;
            if (resultTargetTile.Count != 0)
            {
                normalTargetTile = resultTargetTile["normal"].As<Vector3>();
            }

            // normal of cube facing direction, to calculate angle 
            Vector3 positionCube = this.GlobalPosition;
            PhysicsRayQueryParameters3D rayQueryCube = PhysicsRayQueryParameters3D.Create(positionCube + (this.PendingRotationDirection * 3), positionCube, LAYER2_CUBE);
            Dictionary resultCube = this.GetWorld3D().DirectSpaceState.IntersectRay(rayQueryCube);
            Vector3 normalCube = Vector3.Zero;
            if (resultCube.Count != 0)
            {
                normalCube = resultCube["normal"].As<Vector3>();
            }

            // normal of current tile, for rotation axis
            PhysicsRayQueryParameters3D rayQueryCurrentTile = PhysicsRayQueryParameters3D.Create(positionCube + (Vector3.Up * 5), positionCube + (Vector3.Down * 5), LAYER13_TILE);
            Dictionary resultCurrentTile = this.GetWorld3D().DirectSpaceState.IntersectRay(rayQueryCurrentTile);
            Vector3 normalCurrentTile = Vector3.Zero;
            if (resultCurrentTile.Count != 0)
            {
                normalCurrentTile = resultCurrentTile["normal"].As<Vector3>();
            }
            Vector3 pivot = Vector3.Inf;
            Array<Node> targetCellStaticBody = this.PendingRotationTargetCell.FindChildren("*", "StaticBody3D");
            Vector3 approxPivotLocation = this.PendingRotationTargetCell.GlobalTransform.Origin - (this.PendingRotationDirection * 0.5f * Dimension);


            // use the closest tile `exit` to the direction we're going as the pivot point to rotate the cube.
            // this is "good enough", even though we are not using the cube itself to find its bottom edge to pivot around, which is tricky when we are already on a slope!
            foreach (StaticBody3D staticBody in targetCellStaticBody)
            {
                if (staticBody.IsInGroup("grid-cell__exit"))
                {
                    float currentDistance = pivot.DistanceTo(approxPivotLocation);
                    float staticBodyDistance = staticBody.GlobalPosition.DistanceTo(approxPivotLocation);
                    if (staticBodyDistance < currentDistance)
                    {
                        pivot = staticBody.GlobalPosition;
                    }
                }
            }

            if (normalTargetTile != Vector3.Zero && normalCube != Vector3.Zero && normalCurrentTile != Vector3.Zero && pivot != Vector3.Inf)
            {
                bool isX = this.PendingRotationDirection.X != 0;
                Vector2 normTile = new Vector2(isX ? normalTargetTile.X : normalTargetTile.Z, normalTargetTile.Y).Normalized();
                Vector2 normCube = new Vector2(isX ? normalCube.X : normalCube.Z, normalCube.Y).Normalized();
                // float angle = MathR.DeltaAngleDegrees(0, Mathf.Abs(Mathf.RadToDeg(normTile.AngleTo(normCube))));
                float angle = MathR.DeltaAngleDegrees(0, 180 - Mathf.Abs(Mathf.RadToDeg(normTile.AngleTo(normCube))));
                // angle = 90;
                Vector3 rotationAxis = normalCurrentTile.Cross(this.PendingRotationDirection).Normalized();

                this.RotateCube(pivot, rotationAxis, angle);
            }
            this.PendingRotationTargetCell = null;
            this.PendingRotationDirection = Vector3.Zero;

        }
    }
    public void RotateCube(Vector3 pivot, Vector3 rotationAxis, float targetAngle = 90)
    {
        if (IsMoving) return;
        IsMoving = true;
        // Vector3 pivot = this.GlobalTransform.Origin + (Vector3.Down * 0.5f * Dimension) + (direction * 0.5f * Dimension);

        Transform3D startTransform = this.GlobalTransform;

        Tween tween = this.CreateTween();
        tween.TweenMethod(Callable.From((float progress) => this.LerpTransform(progress, startTransform, pivot, rotationAxis, targetAngle)), 0.0, 1.0, Speed);
        tween.TweenCallback(Callable.From(() => IsMoving = false));
        IsMoving = true;

    }

}
