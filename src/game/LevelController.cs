namespace CubeWorld;

using Godot;

public partial class LevelController : Node
{
    private GridManager GridManager;
    private PlayerCubeController PlayerCubeController;

    public override void _Ready()
    {
        this.GridManager = GDExtra.GetChildrenOfType<GridManager>(this, true)[0];
        this.PlayerCubeController = GDExtra.GetChildrenOfType<PlayerCubeController>(this, true)[0];

        PlayerCubeController.TileRequestedAtGobalPosition += this.GetTileAtPosition;

        Godot.Collections.Array<ComponentPathing> componentPathings = GDExtra.GetChildrenOfType<ComponentPathing>(this, true);
        foreach (var item in componentPathings)
        {
            item.PathRequested += this.GetPointPath;
        }
    }
    public override void _ExitTree()
    {
        PlayerCubeController.TileRequestedAtGobalPosition -= this.GetTileAtPosition;

        Godot.Collections.Array<ComponentPathing> componentPathings = GDExtra.GetChildrenOfType<ComponentPathing>(this, true);
        foreach (var item in componentPathings)
        {
            item.PathRequested -= this.GetPointPath;
        }
    }
    private void GetTileAtPosition(Vector3 targetGlobalPosition, Callable mthd)
    {
        GridCell found = this.GridManager.GetGridCellAtPosition(targetGlobalPosition);
        mthd.Call(found);
    }
    private void GetPointPath(Vector3 fromGlobalPosition, Vector3 targetGlobalPosition, Callable mthd)
    {
        mthd.Call(this.GridManager.GetPointPath(fromGlobalPosition, targetGlobalPosition));
    }
}
