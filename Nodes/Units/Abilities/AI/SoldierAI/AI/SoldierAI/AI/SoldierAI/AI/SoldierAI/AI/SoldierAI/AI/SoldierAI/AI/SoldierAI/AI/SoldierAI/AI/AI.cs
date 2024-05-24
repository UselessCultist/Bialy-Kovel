using Godot;
using System;

public partial class AI : Node
{
	enum State { ON, OFF }

    Character _unit;
	State _state = State.ON;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (GetParent() is Character)
		{
			_unit = GetParent() as Character;
		}
		else 
		{
			throw new Exception("Parent can't use abilities");
		}

        _unit.GetAbility<AttackAbility>().GetAttackArea.AreaEntered += (Area2D area) => 
		{
			if (area.GetParent() is Character unit) 
			{
				if (unit.Type != Type.Resource && unit.PlayerOwner.ID != _unit.PlayerOwner.ID) 
				{
					_unit.PlayerOwner.CommandAttackUnit(unit,true);
                }
			}
		};
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_state == State.OFF) { return; }




        if (_unit.InProcess == null) 
		{
			_unit.InvokeNext();
		}
    }
}