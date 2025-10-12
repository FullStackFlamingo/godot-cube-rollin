using System.Linq;
using Godot;

namespace CubeWorld
{
    public partial class PlayerController : MeshInstance3D
    {
        [Export]
        private GridManager GridManager;


        public override void _Ready()
        {
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            // Handle mouse click.
            if (@event is InputEventMouseButton btn && btn.ButtonIndex == MouseButton.Left && btn.Pressed)
            {
                // _unitLayer.HandleClick(Grid.ScreenToGrid(btn.Position));
                GetViewport().SetInputAsHandled();
                return;
            }
        }

        private void MoveAlongPath(Vector3 targetPosition)
        {
            Vector3[] points = GridManager.GetPathPoints(this.Position, targetPosition);

            Tween tween = CreateTween();
            for (int i = 0; i < points.Count(); i++)
            {
                Vector3 point = points[i];
                Vector3 PrevPoint = Vector3.Zero;
                if (i != 0)
                {
                    PrevPoint = points[i - 1];
                }

                Vector2 dir = new Vector2(PrevPoint.X - point.X, PrevPoint.Z - point.Z);
                // this.RotationDegrees = Vector3.Up * Mathf.LerpAngle(YOUR_NODE.Rotation.y, Mathf.Atan2(OTHER_NODE.Translation.x - this.Position.X, OTHER_NODE.Translation.z - this.Position.Z), 1f);
                // tween.TweenProperty(this, "rotation", new Vector3(0, dir.Angle(), 0), 0.2);
                tween.TweenMethod(
                    Callable.From((Vector3 target) =>
                    {
                        this.LookAt(target);
                    })
                    , point + Vector3.Up, point + Vector3.Up, 0.3f);
                tween.Parallel().TweenProperty(this, "position", point + Vector3.Up, 1);
                // tween.Parallel().TweenMethod(
                //     Callable.From((Vector3 target) =>
                //     {
                //         Vector3 rot = this.Rotation;
                //         rot.Y = Mathf.Atan2(target.X, target.Z);
                //         this.Rotation = rot;
                //     }), PrevPoint, point, 0.2f);

                // tween.Parallel().TweenMethod(
                //     Callable.From((Vector3 target) =>
                //     {
                //         this.LookAt(target, Vector3.Up);
                //     })
                //     , new Vector3(PrevPoint.X, this.Position.Y, PrevPoint.Z), new Vector3(point.X, this.Position.Y, point.Z), 0.2f);
                // tween.TweenMethod(Callable.From((float progress) => LerpTransform(progress, startTransform, rotationCenter, rotationAxis)), 0.0, 1.0, 1);
            }
        }

    }
}
