namespace CubeWorld;

using System;
using Godot;


public partial class ComponentPathing : Node
{
    [Signal]
    public delegate void PathRequestedEventHandler(Vector3 from, Vector3 target, Callable mthd);

    private Node3D Parent;

    private bool busy = false;

    public override void _Ready()
    {
        this.Parent = this.GetParent<Node3D>();
    }

    public void RequestPath(Vector3 fromGlobalPosition, Vector3 targGlobaletPosition)
    {
        this.EmitSignal("PathRequested", fromGlobalPosition, targGlobaletPosition, Callable.From((Vector3[] points) => this.ApplyPath(points)));
    }

    private void ApplyPath(Vector3[] points)
    {
        if (this.busy) return;
        // must have at least 1 points to move between, and Parent to exist
        if (points.Length < 2 || this.Parent == null) return;
        this.busy = true;
        Tween tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
        // `point 0` should be Parent.Position anyway, so start at `point 1`
        for (int i = 1; i < points.Length; i++)
        {
            Vector3 point = points[i];
            Vector3 PrevPoint = Vector3.Zero;
            if (i != 0)
            {
                PrevPoint = points[i - 1];
            }

            Vector2 dir = new Vector2(PrevPoint.X - point.X, PrevPoint.Z - point.Z).Normalized();

            tween.TweenMethod(
                Callable.From((float progress) =>
                {
                    float shortestRotation = MathR.DeltaAngle(this.Parent.Rotation.Y, Mathf.Atan2(dir.X, dir.Y));
                    GD.Print("dir: " + dir + " , shortestRotation: " + shortestRotation);
                    //https://forum.godotengine.org/t/lerping-a-2d-angle-while-going-trought-the-shortest-possible-distance/24124/3
                    Vector3 TargetRotation = new Vector3(this.Parent.Rotation.X, this.Parent.Rotation.Y + shortestRotation, this.Parent.Rotation.Z);

                    this.Parent.Rotation = this.Parent.Rotation.Lerp(TargetRotation, progress);
                    this.Parent.Position = this.Parent.Position.Lerp(point + Vector3.Up, progress);
                })
                , 0f, 1f, 0.3f);
        }
        tween.Finished += () => this.busy = false;
    }
}
