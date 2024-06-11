using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class Collision : Node2D
{
    public static Thread CollisionThread;
    Character _character;
    // -1 обозначает точку центра что обозначает Position, 1 обозначает точку принадлежащую юниту, 0 никому не принадлежит
    [Export] ushort cell_width = 0;
    [Export] ushort cell_height = 0;
    // отвечает за действие с коллизией (физикой)
    Area2D _area = new();
    CollisionShape2D _collision = new();
    [Export]Shape2D shape = null;
    TileMapAstar2D _tile_map;
    List<Vector2I> _global_collision_cells = new();

    public Area2D CollisionArea { get { return _area; } }

    public Collision() 
    {

    }

    public Collision(ushort height, ushort width, Shape2D shape) 
    {
        cell_height = height;
        cell_width = width;
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

    void updateCells() 
    {
        _global_collision_cells = new();
        Vector2I point_global_coord = new
            (
                (int)Math.Floor(_character.Position.X / 16),
                (int)Math.Floor(_character.Position.Y / 16)
            );

        int max_height = (int)Math.Ceiling(cell_height/2d);
        int min_height = cell_height-max_height;

        int max_width = (int)Math.Ceiling(cell_width/2d);
        int min_width = cell_width - max_width;

        for (int i = -min_height; i < max_height; i++)
        {
            for (int j = -min_width; j < max_width; j++)
            {
                _global_collision_cells.Add(point_global_coord + new Vector2I(j,i));
            }
        }
    }

    Vector2I getCenterLocalPos() 
    {
        Vector2I pos = new(
            cell_height / 2,
            cell_width / 2
        );

        return pos;
    }

    public void SolidCenterCellZone()
    {
        Vector2I center = new
        (
            (int)Math.Floor(_character.Position.X / 16),
            (int)Math.Floor(_character.Position.Y / 16)
        );
        _tile_map.Grid.SetPointSolid(center, true);
    }

    public void UnsolidCenterCellZone()
    {
        Vector2I center = new
        (
            (int)Math.Floor(_character.Position.X / 16),
            (int)Math.Floor(_character.Position.Y / 16)
        );
        _tile_map.Grid.SetPointSolid(center, false);
    }

    public void SolidUnitCellZone()
    {
        foreach (var cell in _global_collision_cells)
        {
            ulong l = _character.GetRid().Id;
            _tile_map.MakeCellObjectID(cell, l);

            _tile_map.Grid.SetPointSolid(cell, true);
        }
    }

    public void UnsolidUnitCellZone() 
    {
        foreach (var cell in _global_collision_cells)
        {
            _tile_map.MakeCellObjectID(cell, 0);
            _tile_map.MakeCellEndOfTarget(cell, false);
            _tile_map.Grid.SetPointSolid(cell, false);
        }
    }

    /*static public bool IsCellZoneClear()
    {
        Vector2I local_cell_center_pos = getCenterLocalPos();

        if (cell_height > 0)
        {
            for (int i = 0; i < cell_width; i++)
            {
                for (int j = 0; j < cell_width; j++)
                {
                    Vector2I point_global_coord = zero_arr_point_coord + new Vector2I(j, i);
                    TileData data = _tile_map.GetCellTileData(point_global_coord);

                    bool walkable = (bool)data.GetCustomDataByLayerId(0);
                    if (!walkable)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }*/

    private void changeCollisionPosition() 
    {
        UnsolidUnitCellZone();
        updateCells();
        SolidUnitCellZone();
        //QueueRedraw();
    }

    public override void _Ready()
    {
        Game game = GetGameNode();
        if (game == null) { throw new Exception("Game is null"); }
        _tile_map = GetNode<TileMapAstar2D>("../../../TileMapAstar2D");

        if (_tile_map == null)
        {
            throw new Exception("TileMapAstar2D in world is not exiest");
        }

        _character = GetParent<Character>();
        MoveAbility ability = _character.GetAbility<MoveAbility>();
        if (ability != null) 
        {
            ability.OffCharacterCollision += UnsolidUnitCellZone;
            ability.OnCharacterCollision += () =>
            {
                updateCells();
                SolidUnitCellZone();
            };
        }

        HealthAbility health = _character.GetAbility<HealthAbility>();
        if (health != null)
        {
            health.DieEvent += UnsolidUnitCellZone;
        }

        if (shape != null)
        {
            _collision.Shape = shape;
            _area.AddChild(_collision);
        }

        _area.Monitorable = true;
        AddChild(_area);

        changeCollisionPosition();
    }

    public override void _Draw()
    {
        /*Vector2I pos = new
        (
            -8,
            -8
        );

        Vector2I correction = new
        (
            (int)Math.Floor(cell_width / 2d)*16,
            (int)Math.Floor(cell_height / 2d)*16
        );

        DrawRect(new(pos-correction, cell_width*16, cell_height*16), Color.Color8(255, 0, 0));*/
    }
}
