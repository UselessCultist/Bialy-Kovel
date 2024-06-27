using Godot;
using System;

public partial class HistoryCloseButton : TextureButton
{
    Control control;
    public override void _Ready()
    {
        control = GetNode<Control>("../");
    }
    public override void _Pressed()
    {
        control.Visible = !control.Visible;
    }
}
