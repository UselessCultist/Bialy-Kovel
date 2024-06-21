using Godot;
using System.Collections.Generic;

public enum PlayerState { Computer, Player, OFF }

public partial class Player : Node2D
{
    public Player() { }

    public Player(int ID, PlayerState state)
    {
        _playerID = ID;
        _state = state;
    }

    public List<Character> _selectedUnits = new();

    int[] _resource = new int[(int)ResourceType.MAX];
    bool shift = false;
    [Export] int _playerID = 1;
    [Export] PlayerState _state = PlayerState.Player;
    Cursor _cursor;
    Game game;

    public Cursor Cursor { get { return _cursor; } }


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

        var list_targets = game.TileMap.GetFreeEndCellForManyUnits((Vector2I)(target / 16), _selectedUnits.Count);

        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            target = list_targets[i] * 16;
            Character c = _selectedUnits[i];
            if (!c.GetRid().IsValid) { continue; }

            game.TileMap.MakeCellEndOfTarget(list_targets[i], true);

            var command = new CommandMoveTo(c, target);
            if (set_add) { c.SetCommand(command); } else { c.AddCommand(command); };
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

        game = GetNode<Game>("../");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state == PlayerState.Computer) { return; }

        shift = Input.IsKeyPressed(Key.Shift);

        if (Input.IsActionJustPressed("right_click"))
        {
            if (_cursor.GetUnitUnderCursor()  != null)
            {
                Character _unitUnderCursor = _cursor.GetUnitUnderCursor();

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
                CommandMoveSelectedUnits(GetGlobalMousePosition(), !shift);
            }
        }
    }




}
