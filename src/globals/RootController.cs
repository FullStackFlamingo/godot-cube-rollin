namespace CubeWorld;

using Godot;
using System;

public partial class RootController : Node
{
    public Camera3D GameCamera { get; private set; }

    public override void _Ready()
    {
        // this.GameCamera = (Camera3D)FindChild("GameCamera", true);

    }

}
