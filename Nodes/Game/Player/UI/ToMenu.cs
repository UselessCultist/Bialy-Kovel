using Godot;
using System;

public partial class ToMenu : Button
{
    PackedScene newScene;
    public override void _Ready()
    {
        newScene = (PackedScene)ResourceLoader.Load("res://Nodes/Game/MainScene/Main.tscn");
    }

    public override void _Pressed()
    {
        GetTree().ChangeSceneToPacked(newScene);
    }
}
