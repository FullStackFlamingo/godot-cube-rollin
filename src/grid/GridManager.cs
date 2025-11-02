namespace CubeWorld;

using Godot;
using Godot.Collections;


public partial class GridManager : Node3D
{
    private int TileWidth = 2;

    private System.Collections.Generic.Dictionary<Vector2I, GridCell> cellIndices = new();

    private AStar3DCustom aStar3D = new();
    public readonly Vector2I[] Directions = {
        Vector2I.Left,
        Vector2I.Right,
        Vector2I.Up,
        Vector2I.Down
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
            cellIndices[new((int)cell.Position.X, (int)cell.Position.Z)] = gridCell;
        }
        foreach (Vector2I cellPos in cellIndices.Keys)
        {
            foreach (GridCell neighbor in FindNeighbors(cellPos, cellIndices))
            {
                aStar3D.ConnectPoints(cellIndices[cellPos].AStarPointId, neighbor.AStarPointId);
            }
        }

    }

    /// <summary>
    /// Generate path from Global Position to another Global Position
    /// </summary>
    /// <param name="fromGlobalPosition"></param>
    /// <param name="toGlobalPosition"></param>
    /// <returns>An array of points</returns>
    public Vector3[] GetPathPoints(Vector3 fromGlobalPosition, Vector3 toGlobalPosition)
    {
        GridCell gridCellA = cellIndices[PositionToCell(fromGlobalPosition)];
        GridCell gridCellB = cellIndices[PositionToCell(toGlobalPosition)];
        if (gridCellA == null) GD.PrintErr("GridCell not found at fromGlobalPosition" + fromGlobalPosition.ToString());
        if (gridCellB == null) GD.PrintErr("GridCell not found at toGlobalPosition" + toGlobalPosition.ToString());
        if (gridCellA == null || gridCellB == null) return [];

        return aStar3D.GetPointPath(gridCellA.AStarPointId, gridCellB.AStarPointId);
    }

    /// <summary>
    /// Convert global position to cell position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2I PositionToCell(Vector3 globalPosition)
    {
        Vector3 converted = (globalPosition / TileWidth).Floor();
        return new Vector2I((int)converted.X, (int)converted.Z);
    }


    // public Vector3 CellToPosition(Vector3I cellPosition)
    // {
    //     Vector3 output = Vector3.Zero;
    //     output.X = cellPosition.X* TileWidth;
    //     output.Y = cellPosition.Y;
    //     output.Z = cellPosition.Z* TileWidth;
    //     return new Vector3(cellPosition) * TileWidth + (TileWidth / 2);
    // }

    /// <summary>
    /// In each direction, check if the neighbors of the cellPos are "connected". 
    /// Return "connected" cells as List.
    /// </summary>
    /// <param name="cellPos">position of "current" cell</param>
    /// <param name="cellIndices">All cells</param>
    /// <returns></returns>
    private System.Collections.Generic.List<GridCell> FindNeighbors(Vector2I cellPos, System.Collections.Generic.Dictionary<Vector2I, GridCell> cellIndices)
    {

        System.Collections.Generic.List<GridCell> result = new();
        foreach (Vector2I direction in Directions)
        {
            // for this direction, the "neighbor" is a TileWidth away from the current cellPos
            Vector2I neighborPos = cellPos + (direction * TileWidth);
            // No neighbor? we're done!
            if (!cellIndices.ContainsKey(neighborPos)) { continue; }
            GridCell gridCellNeighbor = cellIndices[neighborPos];
            GridCell gridCellSelf = cellIndices[cellPos];

            // Already connected? We're done!
            if (aStar3D.ArePointsConnected(gridCellSelf.AStarPointId, gridCellNeighbor.AStarPointId)) { continue; }


            // grab all the "exit" nodes from this cell
            Array<Node> cellExits = gridCellSelf.Node.FindChildren("*", "StaticBody3D");
            // grab all the "exit" nodes from neighbor cell
            Array<Node> neighborExits = gridCellNeighbor.Node.FindChildren("*", "StaticBody3D");
            // compare exits from each cell, if we have an overlapping position, this means the cells are "connected".
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
            // add to returned List result
            if (shouldConnect) result.Add(gridCellNeighbor);

        }
        return result;
    }
}

