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
    int _playerID = 1;
    PlayerState _state = PlayerState.Player;
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

	public void CommandMoveSelectedUnits(Vector2 target)
	{
		if (_selectedUnits.Count == 0) { return; };
		foreach (Character c in _selectedUnits) 
		{
			var command = new CommandMoveTo(c, target);
			c.AddCommand(command);
		}
	}

    public void CommandMoveToUnit(Character target)
    {
        if (_selectedUnits.Count == 0) { return; };
        foreach (Character c in _selectedUnits)
        {
            var command = new CommandMoveToUnit(c, target);
            c.AddCommand(command);
        }
    }

    public void CommandAttackUnit(Character target)
    {
        if (_selectedUnits.Count == 0) { return; };
        foreach (Character c in _selectedUnits)
        {
            var command = new CommandAttack(c, target);
            c.AddCommand(command);
        }
    }

    public void CommandExtractResource(Character target) 
    {
        if (_selectedUnits.Count == 0) { return; };
        foreach (Character c in _selectedUnits)
        {
            var command = new CommandExtract(c, target);
            c.AddCommand(command);
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
        if (Input.IsActionJustPressed("left_click")) 
		{
			if (_cursor.UnderCursor is Character) 
			{
                Character _unitUnderCursor = _cursor.UnderCursor as Character;
                if (_unitUnderCursor.PlayerOwner.ID == _playerID) 
                {
                    _selectedUnits.Clear();
                    _selectedUnits.Add(_unitUnderCursor);
                }
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
                        CommandAttackUnit(_unitUnderCursor);
                    }
                    else 
                    {
                        CommandMoveToUnit(_unitUnderCursor);
                    }
                }
                else
                {
                    CommandExtractResource(_unitUnderCursor);
                }
            }
            else 
            {
                CommandMoveSelectedUnits(_cursor.Position);
            }
        }
    }
}
