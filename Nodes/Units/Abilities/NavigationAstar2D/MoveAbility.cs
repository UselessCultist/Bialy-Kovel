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
    public CommandMoveToUnit(Character character, Character target)
    {
        _character = character;
        _target = target;
        _ability = _character.GetAbility<MoveAbility>();
        if (_ability == null) { throw new Exception("This unit not moveable"); }
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

public partial class MoveAbility : NavigationAgent2D
{
    public event EventHandler End = ()=>{};
    public event EventHandler ChangePosition = ()=>{};
    public event EventHandler ChangePathPoint = ()=>{};
    enum State { IDLE, FOLLOW }

    //const float MASS = 10.0f;
    const float ARRIVE_DISTANCE = 6.5f;

    float _speed = 100;

    State _state = State.IDLE;
    Vector2 _velocity = new Vector2();

    TileMapAstar2D _tile_map;
    Character _character;
    Character _target;
    Collision _collision;

    Vector2 _target_position;
    Vector2[] _path;
    Vector2 _next_point;

    public void Move(Vector2 position) 
    {
        _target_position = position;
        _change_state(State.FOLLOW);
    }

    public void Move(Character target)
    {
        _target = target;
        _collision = _target.GetAbility<Collision>();
        Move(target.Position);
    }

    public void Stop() 
    {
        _change_state(State.IDLE);
        End();
    }

    public bool _move_to(Vector2 local_position)
    {
        /* поворот по радиусу, такое себе, ну его
        var desired_velocity = (local_position - _character.Position).Normalized() * _speed;
        var steering = desired_velocity - _velocity;
        _velocity += steering / MASS;
        _character.Position += _velocity * (float)GetProcessDeltaTime();
        var rotation = _velocity.Angle();
        */

        _velocity = (local_position - _character.Position).Normalized() * _speed;
        _character.Position += _velocity  * (float)GetProcessDeltaTime();
        ChangePosition();

        return _character.Position.DistanceTo(local_position) < ARRIVE_DISTANCE;
    }

    public int DistanceInCells(Vector2I start_pos, Vector2I end_pos) 
    {
        var arr = _tile_map.FindPath(start_pos, end_pos);
        return arr.Length;
    }

    public T FindNearest<T>(List<T> list, Vector2 position) where T : Node2D
    {
        int min = -1;
        T nearest = null;

        foreach (var item in list)
        {
            var value = DistanceInCells((Vector2I)_character.Position, (Vector2I)item.Position);
            if (value < min || min < 0)
            {
                min = value;
                nearest = item;
            }
        }
        return nearest;
    }

    public List<Character> FindNearestCharactersWithAbility<T>(bool FindAlive) where T:Node 
    {
        Game game = GetGameNode();
        var list = Game.GetListNode<Character>(game);
        List<Character> search_list = new List<Character>();


        foreach (var character in list)
        {
            var ability = character.GetAbility<T>();
            if (ability != null)
            {
                if (FindAlive && !HealthAbility.IsDead(character)) 
                {
                    continue;
                }
                search_list.Add(character);
            }
        }


        return search_list;
    }

    public Character FindNearestResource(ResourceType type)
    {
        var list = FindNearestCharactersWithAbility<Resource>(true);
        List<Character> search_list = new List<Character>();


        foreach (var character in list)
        {
            var ability = character.GetAbility<Resource>();
            if (ability.ResourceType != type)
            {
                continue;
            }
            search_list.Add(character);
        }

        return FindNearest(search_list, _character.GlobalPosition);
    }

    void _update_path() 
    {
        if (_collision != null) { _collision.UnsolidCenterCellZone(); }
        _path = _tile_map.FindPath((Vector2I)_character.Position, (Vector2I)_target_position);
        if (_collision != null) { _collision.SolidCenterCellZone(); }
    }

    void _update_target()
    {
        _target_position = _target.Position;
    }

    public void UpdatePath() 
    {
        _update_path();
    }
        
    private void _change_state(State new_state)
    {
        switch (new_state)
        {
            case State.IDLE:
                _target = null;
                _path = new Vector2[]{};
                if (_character.Animation!=null) _character.Animation.Play("idle");
                break;
            case State.FOLLOW:

                UpdatePath();
                if (_path.Length < 1)
                {
                    _change_state(State.IDLE);
                    Stop();
                    return;
                }
                if (_character.Animation != null) _character.Animation.Play("move");
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
    public override void _Ready()
    {
        Game game = GetGameNode();
        if (game == null) { throw new Exception("Game is null"); }

        _character = GetParent<Character>();
        if (_character == null)
        {
            throw new Exception("Main node of character is not CharacterBody2D");
        }

        _tile_map = game.TileMap;
        if (_tile_map == null)
        {
            throw new Exception("TileMapAstar2D in world is not exiest");
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_state != State.FOLLOW) { return; }

        if (_target != null) 
        {
            _update_target();
        }

        var arrived_to_next_point = _move_to(_next_point);
        if (arrived_to_next_point)
        {
            ChangePathPoint();
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