using Godot;

public partial class Root : Node
{
    public static Root Instance { get; private set; }


    public override void _Ready()
    {
        Instance = this;
        GD.Seed("Default Seed".Hash());
    }
}