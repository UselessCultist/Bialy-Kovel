using Godot;
using System;

public partial class PanelContainer : Godot.PanelContainer
{
    
    Character castle;

    public override void _Ready()
    {
        Visible = false;
        castle = (Character)GetTree().GetFirstNodeInGroup("castle");
        castle.GetAbility<HealthAbility>().DieEvent += () => 
        {
            Control parent = GetNode<Control>("../Control");
            parent.Visible = false;
            Visible = true;
        };
    }
}
