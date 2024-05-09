using Godot;
using System;

//================== Pattern Command ==================
// Интерфейс паттерна и важные классы находится в Game.cs
public class CommandMoveNode<T> : ICommand where T : Node2D, IAbilities
{
    T _node;
    Vector2 _velocity;
    MoveNode _ability;
    public CommandMoveNode(T node, Vector2 velocity)
    {
        _node = node;
        _velocity = velocity;
        _ability = _node.GetAbility<MoveNode>();
        if (_ability == null) { throw new Exception("This unit not moveable"); }
    }
    public CommandMoveNode(T node, Vector2 velocity, float speed)
    {
        _node = node;
        _velocity = velocity;
        _ability = _node.GetAbility<MoveNode>();
        if (_ability == null) { throw new Exception("This unit not moveable"); }
        _ability.SetSpeed(speed);
    }

    event EventHandler ICommand.Handler
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    public void Execute()
    {
        _ability.Move(_velocity);
    }

    public void Undo()
    {
        _ability.Stop();
    }
}
//============ Change State Event Handler =============
public delegate void ChangeStateEventHandler();
//=====================================================
public partial class MoveNode : Node
{
    public ChangeStateEventHandler changeStateEvent = ()=>{};
    public MoveNode() { }

    public MoveNode(float speed, Vector2 bound_point_start, Vector2 bound_point_end)
    {
        _speed = speed;
        _bound_point_start = bound_point_start;
        _bound_point_end = bound_point_end;
    }

    enum State { IDLE, FOLLOW }

    float _speed = 200;

    State _state = State.IDLE;
    Vector2 _velocity = new Vector2();

    Node2D _node;
    Vector2 _bound_point_start = new(0, 0);
    Vector2 _bound_point_end = new(0, 0);

    public void SetMoveBounds(Vector2 start_point, Vector2 end_point)
    {
        var buf = start_point;

        var X = Math.Min(buf.X, end_point.X);
        var Y = Math.Min(buf.Y, end_point.Y);
        start_point = new(X, Y);

        X = Math.Max(buf.X, end_point.X);
        Y = Math.Max(buf.Y, end_point.Y);
        end_point = new(X, Y);

        _bound_point_start = start_point;
        _bound_point_end = end_point;
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void Move(Vector2 velocity)
    {
        _velocity = velocity;
        _change_state(State.FOLLOW);
    }

    public void Stop()
    {
        _velocity = new(0, 0);
        _change_state(State.IDLE);
    }

    public void _move_to()
    {
        var velocity = _velocity.Normalized() * _speed;
        var position = _node.Position + velocity * (float)GetProcessDeltaTime();
        if (_bound_point_start != _bound_point_end)
        {
            var X = Math.Min(position.X, _bound_point_end.X);
            var Y = Math.Min(position.Y, _bound_point_end.Y);
            X = Math.Max(X, _bound_point_start.X);
            Y = Math.Max(Y, _bound_point_start.Y);
            position = new(X, Y);
        }

        _node.Position = position;
    }

    private void _change_state(State new_state)
    {
        switch (new_state)
        {
            case State.IDLE:
                break;
            case State.FOLLOW:
                if (_velocity == Vector2.Zero)
                {
                    _change_state(State.IDLE);
                }
                return;
        }

        _state = new_state;
        changeStateEvent();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _node = GetNode<Node2D>("../");

        if (_node == null)
        {
            throw new Exception("Main node is not Node2D");
        }
        _change_state(State.IDLE);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_state != State.FOLLOW) { return; }

        _move_to();
    }
}
