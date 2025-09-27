using Godot;
using Godot.Collections;

namespace CubeWorld
{
    public partial class GridManager : Node3D
    {
        [Export]
        private int TileWidth = 2;

        private int maxGridSize = 20;
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
                    // System.Console.WriteLine("connect" + cellIndices[cellPos].AStarPointId + "with" + neighbor.AStarPointId);

                    aStar3D.ConnectPoints(cellIndices[cellPos].AStarPointId, neighbor.AStarPointId);
                }
            }

            // System.Console.WriteLine("point0 connections" + aStar3D.GetPointConnections(0).Join());
            // System.Console.WriteLine(aStar3D.GetPointPath(cellIndices[new(0, 0, 0)].AStarPointId, cellIndices[new(10, 0, 12)].AStarPointId).Join());


        }

        private System.Collections.Generic.List<GridCell> FindNeighbors(Vector3I cellPos, System.Collections.Generic.Dictionary<Vector3I, GridCell> cellIndices)
        {
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

            System.Collections.Generic.List<GridCell> result = new();
            foreach (Vector3I direction in Directions)
            {
                Vector3I neighborPos = cellPos + (direction * TileWidth);
                if (!cellIndices.ContainsKey(neighborPos)) { continue; }
                GridCell gridCellNeighbor = cellIndices[neighborPos];
                GridCell gridCellSelf = cellIndices[cellPos];
                if (!aStar3D.ArePointsConnected(gridCellSelf.AStarPointId, gridCellNeighbor.AStarPointId))
                {
                    PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(
                        gridCellNeighbor.Node.Position + Vector3.Up, gridCellNeighbor.Node.Position + Vector3.Down/* , 4096 */);

                    Dictionary result1 = spaceState.IntersectRay(query);
                    if (result1.Count != 0 && result1["normal"].AsVector3() != Vector3.Up)
                    {

                        System.Console.WriteLine("NORMAL" + result1["normal"]);
                        System.Console.WriteLine("CROSS" + result1["normal"].AsVector3().Cross(Vector3.Up));
                        System.Console.WriteLine("ANGLE" + Mathf.RadToDeg(result1["normal"].AsVector3().AngleTo(Vector3.Up)));
                        // FIGURE OUT POINTS ON A PLANE BASED ON NORMAL AND POSITION (equation of a plane)
                    }
                    result.Add(gridCellNeighbor);
                }
            }
            return result;
        }
    }
}
