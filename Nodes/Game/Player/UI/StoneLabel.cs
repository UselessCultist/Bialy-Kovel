using Godot;
using System;

public partial class StoneLabel : Label
{
    Player _player;

    public override void _Ready()
    {
        _player = GetParent().GetParent().GetParent().GetParent().GetParent().GetParent<Player>();
    }

    public override void _Process(double delta)
    {
        Text = _player.GetResource(ResourceType.Stone).ToString();
    }
}
