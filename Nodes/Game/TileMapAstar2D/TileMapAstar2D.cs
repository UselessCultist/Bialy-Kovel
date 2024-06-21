using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    TileData _edit_cell;


    public void MakeCellEndOfTarget(Vector2I cell, bool is_end_of_target) 
    {
        GD.Print(cell);
        var data = GetCellTileData(cell);
        data.SetCustomDataByLayerId(1, is_end_of_target);
    }

    public void MakeCellObjectID(Vector2I cell, ulong object_id)
    {
        var data = GetCellTileData(cell);
        data.SetCustomDataByLayerId(2, object_id);
    }

    bool IsEndOfTarget(Vector2I cell)
    {
        var data = GetCellTileData(cell);
        bool is_end = data.GetCustomDataByLayerId(1).AsBool();
        return is_end;
    }

    bool IsValidObjectInCell(Vector2I cell) 
    {
        var data = GetCellTileData(cell);
        var valid = (ulong)data.GetCustomDataByLayerId(2);
        return valid != 0;
    }

    public void SetCellInfo(Vector2I cell, Character c)
    {
        MakeCellObjectID(cell, c.GetRid().Id);
        MakeCellEndOfTarget(cell, false);
    }

    public void ClearCell(Vector2I cell) 
    {
        MakeCellObjectID(cell, 0);
        MakeCellEndOfTarget(cell, false);
    }

    bool IsCellClear(Vector2I cell) 
    {
        if (Grid.Region.HasPoint(cell)) 
        {
            bool solid = Grid.IsPointSolid(cell);
            bool end = IsEndOfTarget(cell);
            bool obj = IsValidObjectInCell(cell);
            if (solid || end || obj) { return false; }

            return true;
        }
        return false;
    }

    public List<Vector2I> GetFreeEndCellForManyUnits(Vector2I start, int n) 
    {
        List<Vector2I> freeCells = new();

        Queue<Vector2I> queue = new();
        HashSet<Vector2I> visited = new();
        queue.Enqueue(start);
        visited.Add(start);

        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        if (IsCellClear(start))
        {
            freeCells.Add(start);
        }

        /*while ( queue.Count > 0 && freeCells.Count < n ) 
        {
            var vector2I = queue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                Vector2I buf = vector2I + new Vector2I(dx[i], dy[i]);

                if (!visited.Contains(buf) && IsCellClear(buf) && GetCellSourceId(buf) != -1)
                {
                    freeCells.Add(buf);
                    if (freeCells.Count < n) 
                    {
                        break;
                    }
                    queue.Enqueue(buf);
                    visited.Add(buf);
                }
            }
        }*/

        while (queue.Count > 0 && freeCells.Count < n)
        {
            var vector2I = queue.Dequeue();


            for (int i = 0; i < 4; i++)
            {
                Vector2I buf = vector2I + new Vector2I(dx[i], dy[i]);

                if (!visited.Contains(buf) && GetCellSourceId(buf) != -1)
                {
                    if (IsCellClear(buf))
                    {
                        freeCells.Add(buf);
                        if (freeCells.Count < n)
                        {
                            return freeCells;
                        }
                    }
                    queue.Enqueue(buf);
                    visited.Add(buf);
                }
            }
        }

        return freeCells;
    }

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
        _astar.Region = new Rect2I(-100, -100, 250, 200);   
        _astar.CellSize = CELL_SIZE;
        _astar.Offset = new(CELL_SIZE.X*0.5f, CELL_SIZE.Y*0.5f);
        _astar.DefaultComputeHeuristic = AStarGrid2D.Heuristic.Euclidean;
        _astar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Euclidean;
        _astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.AtLeastOneWalkable;
        _astar.Update();

        for (int i = 0; i < _astar.Region.Size.X; i++)
        {
            for (int j = 0; j < _astar.Region.Size.Y; j++)
            {
                Vector2I tile_pos = new 
                (
                    i+_astar.Region.Position.X,
                    j+_astar.Region.Position.Y
                );

                var tile_data = GetCellTileData(tile_pos);
                if ((bool)tile_data.GetCustomDataByLayerId(0) == false) 
                {
                    _astar.SetPointSolid(tile_pos);
                }
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

    }
}