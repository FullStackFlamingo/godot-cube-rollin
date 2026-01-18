namespace CubeWorld;

using Godot;

public partial class Enemy : Node3D
{

    private ComponentPathing ComponentPathing { get; set; }

    private bool LetsMove = false;
    private bool Reversing = false;

    private Vector3 MoveTargetFrom = new Vector3(0, 1.5f, 0);
    private Vector3 MoveTargetTo = new Vector3(16, 1.5f, 8);

    public override void _Ready()
    {
        this.ComponentPathing = (ComponentPathing)this.FindChild("ComponentPathing");

        this.ComponentPathing.GridCellPathMovementFinished += this.OnMovementFinished;
        this.LetsMove = true;
    }

    public override void _Process(double delta)
    {
        if (this.LetsMove)
        {
            if (this.Reversing)
            {
                this.ComponentPathing.RequestPath(this.MoveTargetTo, this.MoveTargetFrom);
            }
            else
            {
                this.ComponentPathing.RequestPath(this.MoveTargetFrom, this.MoveTargetTo);
            }
            this.LetsMove = false;
        }
    }

    public void OnMovementFinished()
    {
        this.Reversing = !this.Reversing;
        this.LetsMove = true;

    }
}
