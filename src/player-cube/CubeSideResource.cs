using Godot;
namespace CubeWorld;

[GlobalClass]
partial class CubeSideResource : Resource
{
    [Export]
    public GridCellState State;
    [Export]
    public Color Color;

    // Make sure you provide a parameterless constructor.
    // In C#, a parameterless constructor is different from a
    // constructor with all default values.
    // Without a parameterless constructor, Godot will have problems
    // creating and editing your resource via the inspector.
    public CubeSideResource() : this(GridCellState.RED, Color.FromHtml("#ff0000"))
    {

    }

    public CubeSideResource(GridCellState state, Color color)
    {
        this.State = state;
        this.Color = color;
    }
}