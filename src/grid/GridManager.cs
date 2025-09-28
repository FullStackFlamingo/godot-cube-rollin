using System.Linq;
using Godot;
using Godot.Collections;

namespace CubeWorld
{
    public partial class GridManager : Node3D
    {
        [Export]
        private MeshInstance3D TestInstance;
        private int TileWidth = 2;

        private System.Collections.Generic.Dictionary<Vector3I, GridCell> cellIndices = new();

        private AStar3DCustom aStar3D = new();
        public readonly Vector3I[] Directions = {
        Vector3I.Left,
        Vector3I.Right,
        Vector3I.Forward,
        Vector3I.Back
    };

        public override void _Ready()
        {
            base._Ready();
            Array<Node> allCells = GetTree().GetNodesInGroup("grid-cell");

            foreach (Node3D cell in allCells)
            {
                long id = aStar3D.GetAvailablePointId();
                GridCell gridCell = new(cell, id);
                aStar3D.AddPoint(id, cell.Position);
                cellIndices[new((int)cell.Position.X, (int)cell.Position.Y, (int)cell.Position.Z)] = gridCell;
            }
            foreach (Vector3I cellPos in cellIndices.Keys)
            {
                foreach (GridCell neighbor in FindNeighbors(cellPos, cellIndices))
                {
                    aStar3D.ConnectPoints(cellIndices[cellPos].AStarPointId, neighbor.AStarPointId);
                }
            }

            Vector3[] points = aStar3D.GetPointPath(cellIndices[new(0, 0, 0)].AStarPointId, cellIndices[new(10, 0, 12)].AStarPointId);

            Tween tween = CreateTween();
            for (int i = 0; i < points.Count(); i++)
            {
                Vector3 point = points[i];
                Vector3 PrevPoint = Vector3.Zero;
                if (i != 0)
                {
                    PrevPoint = points[i - 1];
                }

                tween.TweenProperty(TestInstance, "position", point + Vector3.Up, 1);
                tween.Parallel().TweenMethod(
                    Callable.From((Vector3 target) =>
                    {
                        TestInstance.RotateY(Mathf.Atan2(target.X, target.Z));
                    }), PrevPoint, point, 0.2f);

                // tween.Parallel().TweenMethod(
                //     Callable.From((Vector3 target) =>
                //     {
                //         TestInstance.LookAt(target, Vector3.Up);
                //     })
                //     , new Vector3(PrevPoint.X, TestInstance.Position.Y, PrevPoint.Z), new Vector3(point.X, TestInstance.Position.Y, point.Z), 0.2f);
                // tween.TweenMethod(Callable.From((float progress) => LerpTransform(progress, startTransform, rotationCenter, rotationAxis)), 0.0, 1.0, 1);
            }

        }

        private System.Collections.Generic.List<GridCell> FindNeighbors(Vector3I cellPos, System.Collections.Generic.Dictionary<Vector3I, GridCell> cellIndices)
        {

            System.Collections.Generic.List<GridCell> result = new();
            foreach (Vector3I direction in Directions)
            {
                Vector3I neighborPos = cellPos + (direction * TileWidth);
                if (!cellIndices.ContainsKey(neighborPos)) { continue; }
                GridCell gridCellNeighbor = cellIndices[neighborPos];
                GridCell gridCellSelf = cellIndices[cellPos];
                if (!aStar3D.ArePointsConnected(gridCellSelf.AStarPointId, gridCellNeighbor.AStarPointId))
                {
                    Array<Node> cellExits = gridCellSelf.Node.FindChildren("*", "StaticBody3D");
                    Array<Node> neighborExits = gridCellNeighbor.Node.FindChildren("*", "StaticBody3D");
                    bool shouldConnect = false;
                    foreach (StaticBody3D cellExit in cellExits)
                    {
                        foreach (StaticBody3D neighborExit in neighborExits)
                        {
                            if (cellExit.GlobalPosition.IsEqualApprox(neighborExit.GlobalPosition))
                            {
                                shouldConnect = true;
                                break;
                            }
                        }
                    }
                    if (shouldConnect) result.Add(gridCellNeighbor);
                }
            }
            return result;
        }
    }
}
