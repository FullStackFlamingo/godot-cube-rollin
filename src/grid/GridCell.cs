namespace CubeWorld;

using Godot;

public partial class GridCell : GodotObject
{
    public Node3D Node;
    public long AStarPointId;

    public GridCell(Node3D node, long aStarPointId)
    {
        AStarPointId = aStarPointId;
        Node = node;
    }


}
