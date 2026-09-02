namespace CubeWorld;

using System;
using System.Linq;
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

        foreach (GridCell cell in allCells)
        {
            cell.AStarPointId = aStar3D.GetAvailablePointId();
            aStar3D.AddPoint(cell.AStarPointId, cell.Position);
            cellIndices[new((int)cell.Position.X, (int)cell.Position.Z)] = cell;
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
    public Vector3[] GetPointPath(Vector3 fromGlobalPosition, Vector3 toGlobalPosition)
    {
        GridCell gridCellA = cellIndices.ContainsKey(PositionToCell(fromGlobalPosition)) ? cellIndices[PositionToCell(fromGlobalPosition)] : null;
        GridCell gridCellB = cellIndices.ContainsKey(PositionToCell(toGlobalPosition)) ? cellIndices[PositionToCell(toGlobalPosition)] : null;
        if (gridCellA == null) GD.PrintErr("GridCell not found at fromGlobalPosition" + fromGlobalPosition.ToString());
        if (gridCellB == null) GD.PrintErr("GridCell not found at toGlobalPosition" + toGlobalPosition.ToString());
        if (gridCellA == null || gridCellB == null) return [];

        return aStar3D.GetPointPath(gridCellA.AStarPointId, gridCellB.AStarPointId);
    }

    /// <summary>
    /// Generate path of GridCells from Global Position to another Global Position
    /// </summary>
    /// <param name="fromGlobalPosition"></param>
    /// <param name="toGlobalPosition"></param>
    /// <returns>An Godot Array<GridCell>, </returns>
    public Array<GridCell> GetGridCellPath(Vector3 fromGlobalPosition, Vector3 toGlobalPosition)
    {
        Vector3[] pointPath = this.GetPointPath(fromGlobalPosition, toGlobalPosition);
        // Godot Array is `Variant` compatible, for using in Callable
        Array<GridCell> output = new Array<GridCell>();
        foreach (Vector3 point in pointPath)
        {
            output.Add(this.cellIndices[this.PositionToCell(point)]);
        }
        return output;
    }
    /// <summary>
    /// Convert global position to cell position
    /// </summary>
    /// <param name="globalPosition"></param>
    /// <returns>Vector2I</returns>
    public Vector2I PositionToCell(Vector3 globalPosition)
    {
        return new Vector2I(
            (int)Math.Round(globalPosition.X / (double)TileWidth, MidpointRounding.AwayFromZero) * TileWidth,
            (int)Math.Round(globalPosition.Z / (double)TileWidth, MidpointRounding.AwayFromZero) * TileWidth
        );
    }

    /// <summary>
    /// Get GridCell at global position
    /// </summary>
    /// <param name="globalPosition"></param>
    /// <returns>GridCell</returns>
    public GridCell GetGridCellAtPosition(Vector3 globalPosition)
    {
        Vector2I cellPos = this.PositionToCell(globalPosition);
        if (!this.cellIndices.ContainsKey(cellPos)) return null;
        return this.cellIndices[cellPos];
    }

    // public Vector3 CellToPosition(Vector2I cellPosition)
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
            Array<Node> cellExits = gridCellSelf.FindChildren("*", "StaticBody3D");
            // grab all the "exit" nodes from neighbor cell
            Array<Node> neighborExits = gridCellNeighbor.FindChildren("*", "StaticBody3D");
            // compare exits from each cell, if we have an overlapping position, this means the cells are "connected".
            bool shouldConnect = false;
            foreach (StaticBody3D cellExit in cellExits)
            {
                foreach (StaticBody3D neighborExit in neighborExits)
                {
                    if (cellExit.IsInGroup("grid-cell__exit") && neighborExit.IsInGroup("grid-cell__exit"))
                    {
                        if (cellExit.GlobalPosition.IsEqualApprox(neighborExit.GlobalPosition))
                        {
                            shouldConnect = true;
                            break;
                        }
                    }
                }
            }
            // add to returned List result
            if (shouldConnect) result.Add(gridCellNeighbor);

        }
        return result;
    }
}

