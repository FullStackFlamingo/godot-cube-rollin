namespace CubeWorld;

using Godot;


public enum GridCellState
{
    NONE,
    RED,
    GREEN,
    BLUE,
    YELLOW,
    PURPLE,
    WHITE
}

public partial class GridCell : Node3D
{
    public long AStarPointId;

    [Export]
    public GridCellState State = GridCellState.NONE;

}
