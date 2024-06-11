using Godot;
using System;

public partial class History : Button
{
    TextureRect rect;
    public override void _Ready()
    {
        rect = GetNode<TextureRect>("../../../../TextureHistory");
    }
    public override void _Pressed()
    {
        rect.Visible = !rect.Visible;
    }
}
