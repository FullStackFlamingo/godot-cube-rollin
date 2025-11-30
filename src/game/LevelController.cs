namespace CubeWorld;

using Godot;

public partial class LevelController : Node
{
    private GridManager GridManager;


    public override void _Ready()
    {
        this.GridManager = GDExtra.GetChildrenOfType<GridManager>(this, true)[0];

        Godot.Collections.Array<ComponentPathing> componentPathings = GDExtra.GetChildrenOfType<ComponentPathing>(this, true);
        foreach (var item in componentPathings)
        {
            item.PathRequested += this.GetPointPath;
        }
    }
    public override void _ExitTree()
    {
        Godot.Collections.Array<ComponentPathing> componentPathings = GDExtra.GetChildrenOfType<ComponentPathing>(this, true);
        foreach (var item in componentPathings)
        {
            item.PathRequested -= this.GetPointPath;
        }
    }
    private void GetPointPath(Vector3 fromGlobalPosition, Vector3 targetGlobalPosition, Callable mthd)
    {
        mthd.Call(this.GridManager.GetPointPath(fromGlobalPosition, targetGlobalPosition));
    }
}
