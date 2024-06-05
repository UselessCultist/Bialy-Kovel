using Godot;
using System;

public partial class WoodLabel : Label
{
    Player _player;

    public override void _Ready()
    {
        _player = GetNode<Player>("../../../../../../");
    }

    public override void _Process(double delta)
    {
        Text = _player.GetResource(ResourceType.Wood).ToString();
    }
}
