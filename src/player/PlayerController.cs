namespace CubeWorld;

using Godot;
using Godot.Collections;
using PhantomCamera;

public partial class PlayerController : MeshInstance3D
{
    [Export]
    public GridManager GridManager;


    private PhysicsRayQueryParameters3D PendingScreenClickQuery;
    private Camera3D currentCam;

    public ComponentPathing ComponentPathing { get; private set; }

    public override void _Ready()
    {
        this.currentCam = this.GetViewport().GetCamera3D();
        this.currentCam.GetNode("./PhantomCameraHost").AsPhantomCameraHost().GetActivePhantomCamera().AsPhantomCamera3D().FollowTarget = this;
        this.ComponentPathing = (ComponentPathing)this.FindChild("ComponentPathing");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton @mouseEvent && @mouseEvent.ButtonIndex == MouseButton.Left && @mouseEvent.Pressed)
        {
            this.PendingScreenClickQuery = MathR.ScreenToWorldRayQueryParamters3D(currentCam, @mouseEvent.Position);
            GetViewport().SetInputAsHandled();
        }
        if (@event is InputEventScreenTouch @touchEvent && @touchEvent.Index == 1 && @touchEvent.Pressed)
        {
            this.PendingScreenClickQuery = MathR.ScreenToWorldRayQueryParamters3D(currentCam, @touchEvent.Position);
            GetViewport().SetInputAsHandled();
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        // screen was clicked/touched
        if (this.PendingScreenClickQuery != null)
        {
            Dictionary result = this.GetWorld3D().DirectSpaceState.IntersectRay(this.PendingScreenClickQuery);
            if (result.Count != 0)
            {
                this.ComponentPathing.RequestPath(this.Position, result["position"].As<Vector3>());
            }
            this.PendingScreenClickQuery = null;

        }
    }



}
