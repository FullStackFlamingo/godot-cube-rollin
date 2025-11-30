namespace CubeWorld;

using Godot;
using Godot.Collections;

class GDExtra
{
    public static Array<T> GetChildrenOfType<[MustBeVariant] T>(Node root, bool recursive = false) where T : Node
    {
        Array<T> childArray = new Array<T>();
        foreach (var node in root.GetChildren(true))
        {
            if (node is T castNode)
            {
                childArray.Add(castNode);
            }
            if (recursive)
            {
                Array<T> nestedChildArray = GetChildrenOfType<T>(node, recursive);
                childArray.AddRange(nestedChildArray);
            }
        }

        return childArray;
    }
}