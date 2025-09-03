using Godot;


namespace CubeWorld
{
    [GlobalClass]
    public partial class Grid : Resource
    {
        [Export]
        public Vector2I Size = new(10, 10);

        [Export]
        public Vector2I CellSize = new(2, 2);

        // Convert grid cell coordinates to coordinates in pixels.
        // Returns the center point of the given cell.
        public Vector2 CellToPosition(Vector2I gridCoords)
        {
            return gridCoords * CellSize + (CellSize / 2);
        }

        // Convert coordinates in pixels to grid cell coordinates.
        public Vector2I PositionToCell(Vector2 screenCoords)
        {
            Vector2 converted = (screenCoords / CellSize).Floor();
            return new Vector2I((int)converted.X, (int)converted.Y);
        }

        // Returns 'true' if the given grid coordinates are valid.
        public bool IsWithinBounds(Vector2I coords)
        {
            return (
                coords.X >= 0 &&
                coords.X < Size.X &&
                coords.Y >= 0 &&
                coords.Y < Size.Y
            );
        }

        // Clamp the given coordinates to the bounds of the grid.
        public Vector2I Clamp(Vector2I coords)
        {
            return coords.Clamp(Vector2I.Zero, new(Size.X - 1, Size.Y - 1));
        }

        // Serializes grid coordinates as integers such that each cell is uniquely
        // represented. Useful when dealing with A* pathfinding.
        public int ToIndex(Vector2I coords)
        {
            return coords.X + Size.X * coords.Y;
        }
    }
}
