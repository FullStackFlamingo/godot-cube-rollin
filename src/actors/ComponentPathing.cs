namespace CubeWorld;

using Godot;
using Godot.Collections;

public partial class ComponentPathing : Node
{
    [Signal]
    public delegate void GridCellPathRequestedEventHandler(Vector3 from, Vector3 target, Callable mthd);

    private Node3D Parent;
    [Export]
    /// <summary>
    /// Gizmo/Anchor location normally center of MeshInstance3D, used to set position Y correctly (how to automate this?)
    /// </summary>
    private Vector3 ParentOriginOffset = Vector3.Up;

    private bool busy = false;

    public override void _Ready()
    {
        this.Parent = this.GetParent<Node3D>();
        // this.ParentOriginOffset = Vector3.Up / 2 * GDExtra.GetChildrenOfType<MeshInstance3D>(this.Parent, true)[0].GetAabb().Size.Y;
    }

    public void RequestPath(Vector3 fromGlobalPosition, Vector3 toGlobalPosition)
    {
        this.EmitSignal("GridCellPathRequested", fromGlobalPosition, toGlobalPosition, Callable.From((Array<GridCell> points) => this.ApplyPath(points)));
    }

    private void ApplyPath(Array<GridCell> points)
    {
        if (this.busy) return;
        // must have at least 1 points to move between, and Parent to exist
        if (points.Count < 2 || this.Parent == null) return;
        this.busy = true;
        Tween tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
        // `point 0` should be Parent.Position anyway, so start at `point 1`
        for (int i = 1; i < points.Count; i++)
        {
            // In this loop, pre-caluclation position and rotation of `from` and `to` Vectors, to generate tweens

            Vector3 point = points[i].Position;
            Vector3 prevPoint = Vector3.Zero;
            if (i != 0) prevPoint = points[i - 1].Position;

            // POSITION - TO/FROM
            // tween from this position, either current position or last point position
            Vector3 fromPosition = this.Parent.Position;
            if (i != 0)
            {
                fromPosition = prevPoint + ParentOriginOffset;
            }
            // tween to this next point
            Vector3 toPosition = point + ParentOriginOffset;

            // ROTATION - TO/FROM

            Vector3 prevPrevPoint = this.Parent.Position;
            if (i > 1)
            {
                prevPrevPoint = points[i - 2].Position;
            }
            Vector3 fromRotation = this.Parent.Rotation;
            if (i > 1)
            {
                Vector2 prevDir = new Vector2(prevPrevPoint.X - prevPoint.X, prevPrevPoint.Z - prevPoint.Z).Normalized();
                fromRotation = new Vector3(this.Parent.Rotation.X, Mathf.Atan2(prevDir.X, prevDir.Y), this.Parent.Rotation.Z);
            }


            Vector2 dir = new Vector2(prevPoint.X - point.X, prevPoint.Z - point.Z).Normalized();
            //https://forum.godotengine.org/t/lerping-a-2d-angle-while-going-trought-the-shortest-possible-distance/24124/3
            float shortestRotation = MathR.DeltaAngle(fromRotation.Y, Mathf.Atan2(dir.X, dir.Y));
            Vector3 toRotation = new Vector3(this.Parent.Rotation.X, fromRotation.Y + shortestRotation, this.Parent.Rotation.Z);


            tween.TweenMethod(
                Callable.From((float progress) =>
                {

                    this.Parent.Rotation = fromRotation.Lerp(toRotation, progress);
                    this.Parent.Position = fromPosition.Lerp(toPosition, progress);

                })
                , 0f, 1f, 0.3f);

            // tween.TweenProperty(this.Parent, "position", point + Vector3.Up, 1f);
        }
        tween.Finished += () => this.busy = false;
    }
}
