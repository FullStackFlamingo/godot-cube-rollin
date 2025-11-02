namespace CubeWorld;

using Godot;

public class GridCell
{
    public Node3D Node;
    public long AStarPointId;

    public GridCell(Node3D node, long aStarPointId)
    {
        AStarPointId = aStarPointId;
        Node = node;
    }


}
