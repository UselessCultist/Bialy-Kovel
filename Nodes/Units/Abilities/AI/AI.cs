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