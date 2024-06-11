using Godot;
using System;

public partial class HistoryCloseButton : TextureButton
{
    TextureRect rect;
    public override void _Ready()
    {
        rect = GetNode<TextureRect>("../");
    }
    public override void _Pressed()
    {
        rect.Visible = !rect.Visible;
    }
}
