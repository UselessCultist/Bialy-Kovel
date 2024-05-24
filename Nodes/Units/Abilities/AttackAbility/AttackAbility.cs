using System;
using System.Net.NetworkInformation;
using Godot;

//================== Pattern Command ==================
// Интерфейс паттерна и важные классы находится в Game.cs
public class CommandAttack : ICommand
{
    public event EventHandler Handler = () => { };
    Character _character;
    Character _target;
    AttackAbility _ability;
    public CommandAttack(Character character, Character target)
    {
        _character = character;
        _target = target;
        _ability = _character.GetAbility<AttackAbility>();
        if (_ability == null) { throw new Exception("This unit can't attack"); }
    }

    public void Execute()
    {
        Handler += () =>
        {
            _ability.End = () => { };
        };
        _ability.End += Handler;

        try
        {
            _ability.SetTarget(_target);
        }
        catch
        {
            _ability.End();
        }
    }

    public void Undo()
    {
        _ability.Stop();
    }
}
//=====================================================
public partial class AttackAbility : AbilityWithCommands
{
    public EventHandler End = () => { };
    enum State { REST, FOLLOW, SWING, ATTACK, RELOAD }

    Character _unit;
    Character _target;
    Area2D _attack_area = new();
    Area2D _target_area;
    CollisionShape2D _attack_shape = new();
    [Export] RectangleShape2D _shape;

    int _damage = 10;
    bool _reload = false;
    State _state = State.REST;

    public AttackAbility()
    {

    }

    public AttackAbility(Shape2D AttackShape) 
    {
        _attack_shape.Shape = AttackShape;
    }

    private void _change_state(State new_state)
    {
        switch (new_state)
        {
            case State.REST:
                _unit.Animation.Play("animation/idle");
                break;
            case State.FOLLOW:
                break;
            case State.SWING:
                break;
            case State.ATTACK:
                if (_target != null) 
                {
                    HealthAbility target_health = _target.GetAbility<HealthAbility>();
                    target_health.GetDamage(_damage);
                    if (target_health.IsDead()) { _target = null; End(); }
                }
                Reload();
                return;
            case State.RELOAD:
                break;
        }
        _state = new_state;
    }

    bool _is_in_attack_area() 
    {
        return _attack_area.OverlapsArea(_target_area);
    }

    bool _can_kill_that(Character target) 
    {
        HealthAbility target_health = target.GetAbility<HealthAbility>();

        if (target_health != null)
        {
            if
            (
            !target_health.IsInvulnerable() &&
            !target_health.IsDead()
            )
            {
                return true;
            }
        }
        return false;
    }

    public void SetTarget(Character target) 
    {
        _target = target;
        _target_area = _target.GetAbility<Collision>().CollisionArea;
    }

    async void Attack()
	{
        _change_state(State.SWING);
        _unit.Animation.Play("animation/attack");
        await ToSignal(GetTree().CreateTimer(0.7f), SceneTreeTimer.SignalName.Timeout);
        _change_state(State.ATTACK);
    }

    async void Reload() 
    {
        _reload = true;
        _change_state(State.RELOAD);
        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
        _change_state(State.REST);
        _reload = false;
    }

    public void Stop()
    {
        _change_state(State.REST);
        StopCommands();

        _reload = false;
        _target = null;
    }

    public override void InvokeNext()
    {
        if (_queueCommand.Count > 0 && _inProcess == null)
        {
            _inProcess = _queueCommand.Dequeue();

            EventHandler handler = () => { };
            handler = () =>
            {
                _inProcess = null;
            };
            _inProcess.Handler += handler;

            _inProcess.Execute();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _unit = GetParent<Character>();
        if (_unit == null) { throw new Exception("This object can't use attack ability"); }

        _attack_shape.Shape = _shape;
        _attack_area.AddChild(_attack_shape);

        AddChild(_attack_area);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (_state == State.SWING || _reload || _target == null) 
        {
            return;
        }

        if (!_can_kill_that(_target))
        {
            _target = null;
            End();
        }

        if (_is_in_attack_area())
        {
            Attack();
        }
        else if(_state != State.FOLLOW)
        {
            _change_state(State.FOLLOW);
            CommandMoveToUnit command_move = new CommandMoveToUnit(_unit, _target);
            SetCommand(command_move);
            InvokeNext();
        }
    }
}
