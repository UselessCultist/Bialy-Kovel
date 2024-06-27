using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//================== Pattern Command ==================
// Интерфейс паттерна и важные классы находится в Game.cs
public class CommandMoveTo:ICommand
{
    Character _character;
    Vector2 _target;
    MoveAbility _ability;

    public CommandMoveTo(Character character, Vector2 target) 
    {
        _character = character;
        _target = target;
        _ability = _character.GetAbility<MoveAbility>();
        if (_ability == null) { throw new Exception("This unit not moveable"); }
    }

    public event EventHandler Handler = () => {};

    public void Execute() 
    {
        Handler += () => 
        {
            _ability.End -= Handler;
        };
        _ability.End += Handler;

        try
        {
            _ability.Move(_target);
        }
        catch 
        {
            _ability.Stop();
        }
    }

    public void Undo() 
    {
        _ability.Stop();
    }
}

public class CommandMoveToUnit : ICommand
{
    Character _character;
    Character _target;
    MoveAbility _ability;
    Collision _target_ability;

    public CommandMoveToUnit(Character character, Character target)
    {
        _character = character;
        _target = target;
        _ability = _character.GetAbility<MoveAbility>();
        _target_ability = target.GetAbility<Collision>();
        if (_ability == null) { throw new Exception("This unit not moveable"); }
        if (_target_ability == null) { throw new Exception("Can't move on unit without collision"); }
    }

    public event EventHandler Handler = () => { };

    public void Execute()
    {
        Handler += () =>
        {
            _ability.End -= Handler;
        };
        _ability.End += Handler;

        if (_target == null) { _ability.Stop(); }
        try
        {
            _ability.Move(_target, _target_ability);
        }
        catch
        {
            _ability.Stop();
        }
    }

    public void Undo()
    {
        _ability.Stop();
    }
}

public partial class MoveAbility : NavigationAgent2D
{
    public event EventHandler Start = () => { };
    public event EventHandler End = ()=>{};
    public event EventHandler ChangeCeillPosition = () => { };
    public event EventHandler PathToTargetComplete = () => { };
    public event EventHandler OffCharacterCollision = ()=>{};
    public event EventHandler OnCharacterCollision = ()=>{};
    public event EventHandler ChangeTargetPoint = () => { };

    enum State { IDLE, FOLLOW }

    //const float MASS = 10.0f;
    const float ARRIVE_DISTANCE = 1f;

    [Export]float _speed = 100;

    State _state = State.IDLE;
    Vector2 _velocity = new Vector2();

    TileMapAstar2D _tile_map;
    Character _character;
    Character _target;
    Collision _target_collision;

    Vector2 _target_position;
    Vector2I _target_ceil;
    Vector2[] _path;
    Vector2I _current_ceil;
    Vector2 _next_point;
    Vector2I _next_character_ceil;
    Vector2 _next_pos;

    Area2D _interact_area = new();
    CollisionShape2D _interact_collision = new();
    [Export] RectangleShape2D _interact_shape;

    public void UpdateCurrentCeill() 
    {
        _current_ceil = (Vector2I)_character.GlobalPosition / 16;
    }

    private void UpdateTargetCeill() 
    {
        _target_ceil = (Vector2I)(_target_position/16);
    }

    private void UpdateNextPoint()
    {
        if (_path.Length > 0) 
        {
            _next_point = _path[0];
        }
    }

    private void SetEndPoint(Vector2 point)
    {
        if (_state == State.FOLLOW) 
        {
            Vector2I present_end_cell = _target_ceil;
        }

        Vector2I future_end_cell = (Vector2I)point / 16;

        _target_position = point;
        ChangeTargetPoint();
    }

    public void Move(Vector2 position) 
    {
        SetEndPoint(position);
        _change_state(State.FOLLOW);
    }

    public void Move(Character target, Collision collision)
    {
        _target = target;
        _target_collision = collision;
        Move(target.GlobalPosition);
    }

    public void Stop() 
    {
        _path = null;
        _target = null;
        _change_state(State.IDLE);
        End();
    }

    public void UpdateVelocity() 
    {
        _velocity = (_next_point - _character.GlobalPosition).Normalized() * _speed * (float)GetProcessDeltaTime();
    }

    public bool IsChangeCeil() 
    {
        UpdateVelocity();

        _next_pos = (_character.GlobalPosition + _velocity);
        _next_character_ceil = (Vector2I)(_next_pos / 16);

        bool is_change_ceil = _current_ceil.X != _next_character_ceil.X || _current_ceil.Y != _next_character_ceil.Y;
        return is_change_ceil;
    }

    public bool _move_to()
    {
        bool del_from_grid = false;
        if (IsChangeCeil()) // проверка до смены позиции
        {
            _tile_map.GridManager.RemoveFromGrid(_character);
            del_from_grid = true;
            if (IsSolid(_next_character_ceil))
            {
                Update_path();
                UpdateVelocity();
            }
        }

        _character.MoveAndCollide(_velocity);
        if (IsChangeCeil()) ChangeCeillPosition(); // проверка после смены позиции
        if (del_from_grid) _tile_map.GridManager.AddToGrid(_character);

        return _character.Position.DistanceTo(_next_point) < ARRIVE_DISTANCE;
    }

    public int DistanceInCells(Vector2I start_pos, Vector2I end_pos) 
    {
        var arr = _tile_map.FindPath(start_pos, end_pos);
        return arr.Length;
    }

    public int DistanceInCells<T>(Vector2I start_pos, Vector2I end_pos, T node) where T : Node2D
    {
        if (node is Character character) 
        {
            var collision = character.GetAbility<Collision>();
            collision.UnsolidCenterCellZone();
            var arr = _tile_map.FindPath(start_pos, end_pos);
            collision.SolidCenterCellZone();
            return arr.Length;
        }
        return -1;
    }

    public T FindNearest<T>(List<T> list) where T : Node2D
    {
        int min = -1;
        T nearest = null;

        foreach (var item in list)
        {
            if (item == null) { continue; }
            var value = DistanceInCells((Vector2I)_character.Position, (Vector2I)item.Position, item);
            if (value < min || min < 0)
            {
                min = value;
                nearest = item;
            }
        }
        return nearest;
    }

    public List<Character> FindNearestCharactersWithAbility<T>() where T:Node 
    {
        Game game = GetGameNode();
        var list = Game.GetListNode<Character>(game.GetNode("GameObjects"));
        List<Character> search_list = new List<Character>();


        foreach (var character in list)
        {
            var ability = character.GetAbility<T>();
            if (ability != null)
            {
                search_list.Add(character);
            }
        }


        return search_list;
    }

    public Character FindNearestResource(ResourceType type)
    {
        var list = FindNearestCharactersWithAbility<Resource>();
        List<Character> search_list = new List<Character>();


        foreach (var character in list)
        {
            var ability = character.GetAbility<Resource>();
            var health = character.GetAbility<HealthAbility>();
            if (ability.ResourceType != type || health.IsDead())
            {
                continue;
            }
            search_list.Add(character);
        }

        return FindNearest(search_list);
    }

    public Character FindNearestResource(ResourceType type, Vector2 pos)
    {
        return _tile_map.GridManager.FindNearestResource(type, pos, 10);
    }

    bool IsSolid(Vector2I ceil) 
    {
        bool is_solid = _tile_map.Grid.IsPointSolid(ceil);
        return is_solid;
    }

    void update_target_point() 
    {
        var list_targets = _tile_map.GetFreeEndCellForManyUnits(_target_ceil, 1);
        if (list_targets.Count > 0)
        {
            SetEndPoint(list_targets[0] * 16);
        }
        else
        {
            Stop();
        }
    } 

    void Update_path() 
    {
        bool is_character_null = _target != null && _target_collision != null;
        if (is_character_null) 
        {
            _target_collision.UnsolidCenterCellZone();
        }

        _path = _tile_map.FindPath((Vector2I)_character.Position, _target_position);
        if (is_character_null)
        {
            _target_collision.SolidCenterCellZone();
        }
        UpdateNextPoint();
    }
        
    private void _change_state(State new_state)
    {
        switch (new_state)
        {
            case State.IDLE:
                _target = null;
                _path = new Vector2[]{};
                break;
            case State.FOLLOW:
                Start();
                Update_path();
                if (_path.Length < 1)
                {
                    _change_state(State.IDLE);
                    Stop();
                    return;
                }

                if (_character.Animation != null) _character.Animation.Play("animation/move");
                _next_point = _path[0];
                break;
        }

        _state = new_state;
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

    // Called when the node enters the scene tree for the first time.
    public async override void _Ready()
    {
        ChangeTargetPoint += UpdateTargetCeill;
        ChangeCeillPosition += UpdateCurrentCeill;

        Game game = GetGameNode();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (game == null) { throw new Exception("Game is null"); }

        _character = GetParent<Character>();
        if (_character == null)
        {
            throw new Exception("Main node of character is not CharacterBody2D");
        }
        UpdateCurrentCeill();

        _tile_map = game.TileMap;
        if (_tile_map == null)
        {
            throw new Exception("TileMapAstar2D in world is not exiest");
        }

        if (_interact_shape != null)
        {
            _interact_collision.Shape = _interact_shape;
            _interact_area.AddChild(_interact_collision);
        }
        _interact_area.CollisionMask = 0b00000000_00000000_00000000_00001000;
        GetParent().AddChild(_interact_area);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        OffCharacterCollision();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_state != State.FOLLOW) 
        {
            return;
        }

        if (_target != null) 
        {
            if (_interact_area.OverlapsArea(_target_collision.CollisionArea))
            {
                PathToTargetComplete();
                Stop();
                return;
            }
        }

        if (!_tile_map.IsCellClear(_target_ceil)) 
        {
            update_target_point();
            Update_path();
        }

        var arrived_to_next_point = _move_to();
        if (arrived_to_next_point)
        {
            _path = _path.Skip(1).ToArray();
            if (_path.IsEmpty())
            {
                Stop();
                return;
            }

            _next_point = _path[0];
        }
    }
}