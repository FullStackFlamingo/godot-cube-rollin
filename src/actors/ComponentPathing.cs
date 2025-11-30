namespace CubeWorld;

using Godot;


public partial class ComponentPathing : Node
{
    [Signal]
    public delegate void PathRequestedEventHandler(Vector3 from, Vector3 target, Callable mthd);

    private Node3D Parent;

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
        if (points.Length == 0 || this.Parent == null) return;



        Tween tween = CreateTween();
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 point = points[i];
            Vector3 PrevPoint = Vector3.Zero;
            if (i != 0)
            {
                PrevPoint = points[i - 1];
            }

            tween.TweenMethod(
                Callable.From((Vector3 target) =>
                {
                    this.Parent.LookAt(target);
                })
                , point + Vector3.Up, PrevPoint.MoveToward(point, 1) + Vector3.Up, 0.3f);
            tween.Parallel().TweenProperty(this.Parent, "position", point + Vector3.Up, 0.3f);

        }
    }
}
