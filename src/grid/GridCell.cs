namespace CubeWorld;

using Godot;
using Godot.Collections;

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
    private MeshInstance3D MeshInstance3D;
    private Material Material;
    public static Dictionary<GridCellState, Color> GridCellCubeSideMap = new Dictionary<GridCellState, Color>{
        { GridCellState.NONE, Color.FromHtml("#94948e")},
        { GridCellState.RED, Color.FromHtml("#bb351f")},
        { GridCellState.GREEN, Color.FromHtml("#2f8f31")},
        { GridCellState.BLUE, Color.FromHtml("#3954bb")},
        { GridCellState.YELLOW, Color.FromHtml("#f3c100")},
        { GridCellState.PURPLE, Color.FromHtml("#7500ff")},
        { GridCellState.WHITE, Color.FromHtml("#fff")},
    };

    public override void _Ready()
    {
        this.MeshInstance3D = GDExtra.GetChildrenOfType<MeshInstance3D>(this, true)[0];
        this.Material = this.MeshInstance3D.GetSurfaceOverrideMaterial(0);
        this.UpdateMaterialState();
    }

    public void UpdateMaterialState()
    {
        // TODO: set nice tile texture?
        this.Material.Set("albedo_color", GridCell.GridCellCubeSideMap[this.State]);
    }

}
