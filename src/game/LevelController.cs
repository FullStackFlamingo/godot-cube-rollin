namespace CubeWorld;

using Godot;
using Godot.Collections;

public partial class LevelController : Node
{
    private GridManager GridManager;
    private PlayerCubeController PlayerCubeController;

    public override void _Ready()
    {
        this.GridManager = GDExtra.GetChildrenOfType<GridManager>(this, true)[0];
        this.PlayerCubeController = GDExtra.GetChildrenOfType<PlayerCubeController>(this, true)[0];

        PlayerCubeController.GridCellPathRequested += this.GetGridCellPath;

        Array<ComponentPathing> componentPathings = GDExtra.GetChildrenOfType<ComponentPathing>(this, true);
        foreach (var item in componentPathings)
        {
            item.PathRequested += this.GetPointPath;
        }
    }
    public override void _ExitTree()
    {
        PlayerCubeController.GridCellPathRequested -= this.GetGridCellPath;

        Array<ComponentPathing> componentPathings = GDExtra.GetChildrenOfType<ComponentPathing>(this, true);
        foreach (var item in componentPathings)
        {
            item.PathRequested -= this.GetPointPath;
        }
    }
    private void GetGridCellPath(Vector3 fromGlobalPosition, Vector3 targetGlobalPosition, Callable mthd)
    {
        Array<GridCell> blar = this.GridManager.GetGridCellPath(fromGlobalPosition, targetGlobalPosition);
        mthd.Call(blar);
    }
    private void GetPointPath(Vector3 fromGlobalPosition, Vector3 targetGlobalPosition, Callable mthd)
    {
        mthd.Call(this.GridManager.GetPointPath(fromGlobalPosition, targetGlobalPosition));
    }
}
