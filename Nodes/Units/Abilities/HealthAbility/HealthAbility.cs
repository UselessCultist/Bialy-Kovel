using Godot;
using System;

public partial class HealthAbility : Node
{
    public HealthAbility(int MaxHealth, bool IsInvulnerable) 
    {
        _maxHealth = MaxHealth;
        _health = _maxHealth;
        _isInvulnerable = IsInvulnerable;
    }

    private float _health;
    private float _maxHealth;
    private bool _isInvulnerable = false;
    private bool _isDead = false;

    public bool IsInvulnerable() { return _isInvulnerable; }
    public virtual bool IsDead() { return _isDead; }
    public static bool IsDead(Character character) 
    {
        var health = character.GetAbility<HealthAbility>();
        if (health == null)
        {
            return false;
        }
        if (health.IsDead())
        {
            return false;
        }
        return true; 
    }
    public virtual float GetHeal(float heal)
    {
        var health = _health;
        if (!IsDead())
        {
            health = health + heal > _maxHealth ? _maxHealth : health + heal;
            _health = health;
            return heal;
        }
        return 0;
    }
    public async virtual void KillUnit()
    {
        _isDead = true;
        Character unit = GetParent<Character>();
        SelectArea select = unit.GetAbility<SelectArea>();
        if (select != null) { select.QueueFree(); }

        unit.Visible = false;
        await ToSignal(GetTree().CreateTimer(10.0f), SceneTreeTimer.SignalName.Timeout);
        GetParent().QueueFree();
    }
    public virtual float GetDamage(float damage)
    {
        if (!IsInvulnerable())
        {
            float health = _health - damage;
            if (health <= 0)
            {
                _health = 0;
                KillUnit();
            }
            _health = health;
            return damage;
        }
        else
        {
            return 0;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
