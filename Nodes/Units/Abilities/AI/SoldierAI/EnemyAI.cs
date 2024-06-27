using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

partial class EnemyAI : Node
{
    enum State { ON, OFF }

    Character _unit;
    Character _castle;
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
            if (_unit._queueCommand.Count < 1) 
            {
                var castle = (Character)GetTree().GetFirstNodeInGroup("castle");
                if (castle != null)
                {
                    var attack = new CommandAttack(_unit, castle);
                    _unit.AddCommand(attack);
                }
            }
            _unit.InvokeNext();
        }
    }
}

