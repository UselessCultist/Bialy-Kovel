using Godot;
using System;
using System.Collections.Generic;

public enum PlayerState { Computer, Player, OFF }

public partial class Player : Node
{
    public Player() { }

    public Player(int ID, PlayerState state) 
    {
        _playerID = ID;
        _state = state;
    }

    int[] _resource = new int[(int)ResourceType.MAX];
    bool shift = false;
    [Export] int _playerID = 1;
    [Export] PlayerState _state = PlayerState.Player;
	Cursor _cursor;

    List<Character> _selectedUnits = new();
    public int ID { get { return _playerID; } }

    public int GetResource(ResourceType resource) 
    {
        return _resource[(int)resource];
    }

    public void AddResource(ResourceType resource, int add_value) 
    {
        _resource[(int)resource] += add_value;
    }

	public void CommandMoveSelectedUnits(Vector2 target, bool set_add)
	{
		if (_selectedUnits.Count == 0) { return; };
		foreach (Character c in _selectedUnits) 
		{
			var command = new CommandMoveTo(c, target);
            if (set_add) { c.SetCommand(command);} else { c.AddCommand(command);};
		}
	}

    public void CommandMoveToUnit(Character target, bool set_add)
    {
        if (_selectedUnits.Count == 0) { return; };
        foreach (Character c in _selectedUnits)
        {
            var command = new CommandMoveToUnit(c, target);
            if (set_add) { c.SetCommand(command); } else { c.AddCommand(command); };
        }
    }

    public void CommandAttackUnit(Character target, bool set_add)
    {
        if (_selectedUnits.Count == 0) { return; };
        foreach (Character c in _selectedUnits)
        {
            var command = new CommandAttack(c, target);
            if (set_add) { c.SetCommand(command); } else { c.AddCommand(command); };
        }
    }

    public void CommandExtractResource(Character target, bool set_add) 
    {
        if (_selectedUnits.Count == 0) { return; };
        foreach (Character c in _selectedUnits)
        {
            var command = new CommandExtract(c, target);
            if (set_add) { c.SetCommand(command); } else { c.AddCommand(command); };
        }
    }

    public override void _Ready()
    {
        if (_state == PlayerState.Computer) { return; }
		_cursor = GetNode<Cursor>("Cursor");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state == PlayerState.Computer) { return; }

        shift = Input.IsKeyPressed(Key.Shift);

        if (Input.IsActionJustPressed("left_click")) 
		{
            if (_cursor.UnderCursor is Character)
            {
                Character _unitUnderCursor = _cursor.UnderCursor as Character;
                if (_unitUnderCursor.PlayerOwner.ID == _playerID)
                {
                    if (!shift) _selectedUnits.Clear();
                    _selectedUnits.Add(_unitUnderCursor);
                }
            }
            else 
            {
                _selectedUnits.Clear();
            }
		}

        if (Input.IsActionJustPressed("right_click"))
        {
            if (_cursor.UnderCursor is Character)
            {
                Character _unitUnderCursor = _cursor.UnderCursor as Character;

                if (_unitUnderCursor.Type != Type.Resource) 
                {
                    if (_unitUnderCursor.PlayerOwner.ID != _playerID)
                    {
                        CommandAttackUnit(_unitUnderCursor, !shift);
                    }
                    else 
                    {
                        CommandMoveToUnit(_unitUnderCursor, !shift);
                    }
                }
                else
                {
                    CommandExtractResource(_unitUnderCursor, !shift);
                }
            }
            else 
            {
                CommandMoveSelectedUnits(_cursor.Position, !shift);
            }
        }
    }
}
