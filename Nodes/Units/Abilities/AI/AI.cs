using Godot;
using System;

public partial class AI : Node
{
	enum State { ON, OFF }

    Character _unit;
	State _state = State.ON;

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