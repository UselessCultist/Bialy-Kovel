using Godot;
using System;

public partial class HelpButton : TextureButton
{
    TextureRect rect;
    public override void _Ready()
    {
        rect = GetNode<TextureRect>("../Help");
    }


    public override void _Pressed()
    {
        rect.Visible = !rect.Visible;
    }
}
