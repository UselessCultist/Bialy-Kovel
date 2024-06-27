using Godot;
using System;

public partial class History : Button
{
    Control book;
    public override void _Ready()
    {
        book = GetNode<Control>("../../../../Book");
    }
    public override void _Pressed()
    {
        book.Visible = !book.Visible;
    }
}
