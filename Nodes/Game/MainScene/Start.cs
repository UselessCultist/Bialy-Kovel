using Godot;
using System;
using static System.Formats.Asn1.AsnWriter;

public partial class Start : Button
{
    private PackedScene newScene;

    public override void _Ready()
    {
        newScene = (PackedScene)ResourceLoader.Load("res://Nodes/Game/GameNode/Game.tscn");
    }
    public override void _Pressed()
    {
        GetTree().ChangeSceneToPacked(newScene);
    }
}
