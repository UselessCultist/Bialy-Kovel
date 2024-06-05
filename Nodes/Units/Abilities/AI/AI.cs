using Godot;
using System;

public partial class AI : Node
{
	enum State { ON, OFF }

    Character _unit;
	State _state = State.ON;

	public async override void _Ready()
	{
		if (GetParent() is Character)
		{
			_unit = GetParent() as Character;
		}
		else 
		{
			throw new Exception("Parent can't use abilities");
		}

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		Area2D attack_area = _unit.GetAbility<AttackAbility>().GetAttackArea;

        _unit.GetAbility<AttackAbility>().GetAttackArea.AreaEntered += (Area2D area) => 
		{
			if (area.GetParent() is Character target) 
			{
				if (target.Type != Type.Resource && target.PlayerOwner != _unit.PlayerOwner) 
				{
                    var command = new CommandAttack(_unit, target);
                    _unit.SetCommand(command);
                }
			}
		};
    }

	public override void _Process(double delta)
	{
		if (_state == State.OFF) { return; }




        if (_unit.InProcess == null) 
		{
			_unit.InvokeNext();
		}
    }
}