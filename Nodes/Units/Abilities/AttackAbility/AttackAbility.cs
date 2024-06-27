using System;
using System.Collections.Generic;
using System.Linq;
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
    public event EventHandler AttackEvent = () => { };

    public EventHandler End = () => { };
    public EventHandler CharacterEnter = () => { };
    EventHandler DieHandler = () => { };


    enum State { REST, FOLLOW, SWING, ATTACK, RELOAD }
    public List<Character> visibleEnemies = new List<Character>();
    Character _unit;
    Character _target;
    HealthAbility _target_health;
    Area2D _attack_area;
    Area2D _target_area;
    CollisionShape2D _attack_shape = new();
    [Export] RectangleShape2D _shape;

    [Export] int _damage = 10;
    bool _reload = false;
    State _state = State.REST;

    public Area2D GetAttackArea { get { return _attack_area; } }

    public AttackAbility()
    {

    }

    public void AddVisibleEnemy(Character enemy)
    {
        if (!visibleEnemies.Contains(enemy))
        {
            visibleEnemies.Add(enemy);
        }
    }

    public void RemoveVisibleEnemy(Character enemy)
    {
        if (visibleEnemies.Contains(enemy))
        {
            visibleEnemies.Remove(enemy);
        }
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
        _target_health = _target.GetAbility<HealthAbility>();


        if (!_can_kill_that(_target))
        {
            _target = null;
            End();
            return;
        }

        DieHandler = () => { Stop(); };
        _target_health.DieEvent += DieHandler;
    }

    async void Attack()
	{
        _change_state(State.SWING);
        _unit.Animation.Play("animation/attack");
        await ToSignal(GetTree().CreateTimer(0.7f), SceneTreeTimer.SignalName.Timeout);
        if (_target == null || !_is_in_attack_area()) { return; }
        _change_state(State.ATTACK);
        AttackEvent();
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

        _target_health.DieEvent -= DieHandler;

        _reload = false;
        _target = null;
        End();
    }

    public override void InvokeNext()
    {
        if (_queueCommand.Count > 0 && _inProcess == null)
        {
            _inProcess = _queueCommand.Dequeue();

            EventHandler handler = () => { };
            handler = () =>
            {
                _unit.Animation.Stop();
                _unit.Animation.Play("animation/idle");
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

        _attack_area = new();
        _attack_shape.Shape = _shape;
        _attack_area.AddChild(_attack_shape);

        _attack_area.Monitorable = true;
        _attack_area.Monitoring = true;


        _attack_area.CollisionLayer = 0b00000000_00000000_00000000_00000100;
        _attack_area.CollisionMask =  0b00000000_00000000_00000000_00001010;
        _attack_area.Name = "AttackArea";
        AddChild(_attack_area);

        _attack_area.AreaEntered += (Area2D area)=> 
        {
            if (area.GetParent() is Character enemy)
            {
                if (enemy.Type != Type.Resource && enemy.PlayerOwner != _unit.PlayerOwner)
                {
                    GD.Print("Add");
                    AddVisibleEnemy(enemy);
                }
            }
        };

        _attack_area.AreaExited += (Area2D area) =>
        {
            if (area.GetParent() is Character enemy)
            {
                if (enemy.Type != Type.Resource && enemy.PlayerOwner != _unit.PlayerOwner)
                {
                    GD.Print("Exit");
                    RemoveVisibleEnemy(enemy);
                }
            }
        };
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (_state == State.SWING || _reload) 
        {
            return;
        }

        bool is_null = _target == null;
        bool is_in_area = is_null ? false : _is_in_attack_area();
        bool see_enemies = visibleEnemies.Count > 0;

        // добрались до цели?
        if (is_in_area)
        {
            StopCommands();
            Attack();
        }
        else
        {
            // Мы ещё не идём до цели?
            if (_state != State.FOLLOW && !is_null)
            {
                _change_state(State.FOLLOW);
                CommandMoveToUnit command_move = new CommandMoveToUnit(_unit, _target);
                AddCommand(command_move);
                InvokeNext();
                return;
            }

            // есть противники рядом?
            if (see_enemies)
            {
                var target = visibleEnemies.First();
                if (is_null || target.GetRid() != _target.GetRid()) 
                {
                    var command = new CommandAttack(_unit, target);
                    _unit.PrependCommand(command);
                }

                return;
            }
        }
    }

}
