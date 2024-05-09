using Godot;
using System;
using System.Collections.Generic;

public partial class TileMapAstar2D : TileMapLayer
{
    enum Tile { OBSTACLE, START_POINT, END_POINT }
    readonly Vector2I CELL_SIZE = new Vector2I(16, 16);
    const float BASE_LINE_WIDTH = 3.0f;
    readonly Color DRAW_COLOR = new Color(1, 1, 1, 0.5f);


    AStarGrid2D _astar = new();

    public AStarGrid2D Grid { get { return _astar; } }

	Vector2I _start_point;
	Vector2I _end_point;
	Vector2[] _path;

    public Vector2 RoundLocalPosition(Vector2 local_position) 
    {
        return MapToLocal(LocalToMap(local_position));
    }

    bool IsPointWalkable(Vector2 local_position) 
    {
        var map_position = LocalToMap(local_position);
        if (_astar.IsInBoundsv(map_position)) 
        {
            return !_astar.IsPointSolid(map_position);
        }
        return false;
    }

    public Vector2[] FindPath(Vector2 local_start_point, Vector2 local_end_point)
    {
        Vector2I start_point = LocalToMap(local_start_point);
        Vector2I end_point = LocalToMap(local_end_point);
        var path = _astar.GetPointPath(start_point, end_point, true);

        return path;
    }

    public List<Character> GetObjectsInNeighborCells(Vector2I cell_pos, int radius) 
    {
        List<Character> objects = new List<Character>();
        Vector2I start_cell = new(cell_pos.X-radius, cell_pos.Y-radius);
        Vector2I end_cell = new(cell_pos.X+radius, cell_pos.Y+radius);

        for (int i = start_cell.Y; i <= end_cell.Y; i++)
        {
            for (int j = start_cell.X; j <= end_cell.X; j++)
            {
                TileData data = GetCellTileData(new Vector2I(i, j));

                var path = data.GetCustomData("ObjectInCell").ToString();
                if (!String.Equals(path, ""))
                {
                    Character obj = GetNode<Character>(path);
                    objects.Add(obj);
                }
            }
        }
        return objects;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _astar.Region = new Rect2I(-250, -250, 500, 500);   
        _astar.CellSize = CELL_SIZE;
        _astar.Offset = new(CELL_SIZE.X*0.5f, CELL_SIZE.Y*0.5f);
        _astar.DefaultComputeHeuristic = AStarGrid2D.Heuristic.Euclidean;
        _astar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Euclidean;
        _astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.AtLeastOneWalkable;
        _astar.Update();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}