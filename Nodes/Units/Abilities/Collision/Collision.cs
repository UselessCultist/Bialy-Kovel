using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class Collision : Node2D
{
    public static Thread CollisionThread;
    Character _character;
    // -1 обозначает точку центра что обозначает Position, 1 обозначает точку принадлежащую юниту, 0 никому не принадлежит
    int[][] _sizeInCells;
    // центр
    Vector2I _center;
    Vector2I _zero_point;
    // отвечает за действие с коллизией (физикой)
    Area2D _area = new();
    CollisionShape2D _collision = new();
    [Export]Shape2D shape = null;
    TileMapAstar2D _tile_map;
    List<Vector2I> _collision_cells;

    public Area2D CollisionArea { get { return _area; } }

    public Collision() 
    {
        if (shape != null) 
        {
            _collision.Shape = shape;
        }
    }

    public Collision(int[][] SizeInCells, Shape2D shape) 
    {
        _sizeInCells = SizeInCells;
        _collision.Shape = shape;

        _area.AddChild(_collision);
    }

    public Game GetGameNode()
    {
        Node search = this;
        do
        {
            if (search is Game)
            {
                return search as Game;
            }
            search = search.GetParent();
        }
        while (search != null);
        return null;
    }

    public bool ShapeIsNull() 
    {
        return _collision.Shape == null;
    }

    void updateCenter() 
    {
        _center = (Vector2I)(_character.Position / 16).Floor();

        Vector2I local_cell_center_pos = getCenterLocalPosFromSizeMap();
        _zero_point = getZeroPointGlobalCellPos(local_cell_center_pos);
    }

    void updateCells() 
    {
        _collision_cells = new();
        if (_sizeInCells.Length > 0)
        {
            for (int i = 0; i < _sizeInCells.Length; i++)
            {
                int[] row = _sizeInCells[i];
                for (int j = 0; j < row.Length; j++)
                {
                    int cell = row[j];
                    if (cell == 0)
                    {
                        continue;
                    }

                    Vector2I point_global_coord = _zero_point + new Vector2I(j, i);
                    _collision_cells.Add(point_global_coord);
                }
            }
        }
    }

    Vector2I getCenterLocalPosFromSizeMap() 
    {
        Vector2I pos = new(-1,-1);
        if (_sizeInCells.Length > 0)
        {
            for (int i = 0; i < _sizeInCells.Length; i++)
            {
                int[] row = _sizeInCells[i];
                for (int j = 0; j < row.Length; j++)
                {
                    if (row[j] == -1)
                    {
                        pos = new(j, i);
                        return pos;
                    };
                }
            }
        }
        throw new Exception("center not exiest");
    }

    Vector2I getZeroPointGlobalCellPos(Vector2I local_cell_center_pos) 
    {
        Vector2I zero_arr_point_coord = _center - local_cell_center_pos;
        return zero_arr_point_coord;
    }

    public void SolidCenterCellZone()
    {
        _tile_map.Grid.SetPointSolid(_center, true);
    }

    public void UnsolidCenterCellZone()
    {
        _tile_map.Grid.SetPointSolid(_center, false);
    }

    public void SolidUnitCellZone()
    {
        string character_path = _character.GetPath();
        foreach (var cell in _collision_cells)
        {
            TileData data = _tile_map.GetCellTileData(cell);

            _tile_map.Grid.SetPointSolid(cell, true);
        }
    }

    public void UnsolidUnitCellZone() 
    {
        foreach (var cell in _collision_cells)
        {
            _tile_map.Grid.SetPointSolid(cell, false);
        }
    }

    public bool IsCellZoneClear()
    {
        Vector2I local_cell_center_pos = getCenterLocalPosFromSizeMap();
        Vector2I zero_arr_point_coord = getZeroPointGlobalCellPos(local_cell_center_pos);

        if (_sizeInCells.Length > 0)
        {
            for (int i = 0; i < _sizeInCells.Length; i++)
            {
                int[] row = _sizeInCells[i];
                for (int j = 0; j < row.Length; j++)
                {
                    int cell = row[j];
                    if (cell == 0)
                    {
                        continue;
                    }

                    Vector2I point_global_coord = zero_arr_point_coord + new Vector2I(j, i);
                    TileData data = _tile_map.GetCellTileData(point_global_coord);

                    var path = data.GetCustomData("objectincell").ToString();
                    if (!String.Equals(path, ""))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private void changeCollisionPosition() 
    {
        SolidUnitCellZone();
        updateCenter();
        updateCells();
        UnsolidUnitCellZone();
    }

    public override void _Ready()
    {
        Game game = GetGameNode();
        if (game == null) { throw new Exception("Game is null"); }

        _tile_map = game.TileMap;
        if (_tile_map == null)
        {
            throw new Exception("TileMapAstar2D in world is not exiest");
        }

        _character = GetParent<Character>();
        MoveAbility ability = _character.GetAbility<MoveAbility>();
        if (ability != null) 
        {
            ability.ChangePathPoint += changeCollisionPosition;
        }

        AddChild(_area);

        updateCenter();
        updateCells();
        SolidUnitCellZone();
    }
}
