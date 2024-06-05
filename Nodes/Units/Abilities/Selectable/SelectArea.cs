using Godot;
using System;

public partial class SelectArea : Area2D
{
    public CollisionShape2D Collision_Shape2D { get; set; } = new();
    [Export] Shape2D Shape { get; set; }
    Character unit;
    bool selected = false;

    public SelectArea(){}

    public void select()
    {
        selected = true;
    }

    public void deselect()
    {
        selected = false;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        unit = GetParent<Character>();
        Collision_Shape2D.Shape = Shape;
        AddChild(Collision_Shape2D);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
