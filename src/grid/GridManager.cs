using Godot;
using System;

public partial class GridManager : Node3D
{
    [Export]
    private MultiMeshInstance3D TileMultiMesh;
    private int TileWidth = 2;

    public override void _Ready()
    {
        base._Ready();

        double gridSize = Math.Sqrt(TileMultiMesh.Multimesh.InstanceCount);
        float tileWidth = TileMultiMesh.Multimesh.Mesh.GetAabb().Size.X;

        int index = 0;
        for (int x = (int)(-gridSize / 2); x < gridSize / 2; x++)
        {
            for (int y = (int)(-gridSize / 2); y < gridSize / 2; y++)
            {
                Transform3D transform = Transform3D.Identity;
                Console.WriteLine("Tile" + index + " " + new Vector3(tileWidth * x, 0, tileWidth * y));
                transform = transform.Translated(new Vector3(tileWidth * x, 0, tileWidth * y));
                TileMultiMesh.Multimesh.SetInstanceTransform(index, transform);
                index++;
            }
        }

    }
}
