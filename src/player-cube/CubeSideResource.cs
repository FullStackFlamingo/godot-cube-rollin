using Godot;
namespace CubeWorld;

[GlobalClass]
partial class CubeSideResource : Resource
{
    [Export]
    private string Name;
    [Export]
    private Color Color;

    // Make sure you provide a parameterless constructor.
    // In C#, a parameterless constructor is different from a
    // constructor with all default values.
    // Without a parameterless constructor, Godot will have problems
    // creating and editing your resource via the inspector.
    public CubeSideResource() : this("Default Name", Color.FromHtml("#ff0000"))
    {

    }

    public CubeSideResource(string name, Color color)
    {
        this.Name = name;
        this.Color = color;
    }
}