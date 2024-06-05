using Godot;
using System;

public partial class HelpButton : TextureButton
{
    TextureRect rect;
    public override void _Ready()
    {
        TextureRect rect = GetNode<TextureRect>("../Help");
        Pressed += ButtonPressed;
    }

    private void ButtonPressed()
    {
        rect.Visible = !rect.Visible;
    }
}
