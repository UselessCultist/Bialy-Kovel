using System;
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
            _ability.Attack(_target);
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
    int _damage = 10;
    float _distance = 16;
    private bool impact { get; set; } = false;
    State _state;

    private void _change_state(State new_state)
    {
        switch (new_state)
        {
            case State.REST:
                _unit.Animation.Play("idle");
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
                _change_state(State.RELOAD);
                return;
            case State.RELOAD:
                break;
        }

        _state = new_state;
    }

    static bool _can_kill_that(Character target) 
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

    public void Attack(Character target)
	{
        _target = target;
        if (_can_kill_that(target))
        {
            if (target.Position.DistanceTo(_unit.Position) <= _distance)
            {
                _change_state(State.SWING);
                _unit.Animation.Play("attack");
            }
            else
            {
                _change_state(State.FOLLOW);
                CommandMoveToUnit command_move = new CommandMoveToUnit(_unit, target);
                SetCommand(command_move);
                InvokeNext();
            }
        }
        else 
        {
            _target = null;
            End();
        }
	}

    public void Stop()
    {
        _change_state(State.REST);
        _target = null;
        impact = false;
    }

    public override void InvokeNext()
    {
        if (_queueCommand.Count > 0 && _inProcess == null)
        {
            _inProcess = _queueCommand.Dequeue();

            EventHandler handler = () => { };
            handler = () =>
            {
                _change_state(State.REST);
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (_state == State.REST && _target != null)
        {
            Attack(_target); 
        }

        if (_state == State.SWING && impact) 
        {
            _change_state(State.ATTACK);
            return; 
        }

        if (_state == State.RELOAD && !impact) 
        {
            _change_state(State.REST);
        }
    }
}
