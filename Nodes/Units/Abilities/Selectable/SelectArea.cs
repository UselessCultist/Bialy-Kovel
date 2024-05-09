using Godot;
using System;

public partial class SelectArea : CollisionShape2D
{
    public SelectArea() 
    {
        var shape = new CapsuleShape2D();
        shape.Height = 32;
        shape.Radius = 7;
        Shape = shape;
    }
    public SelectArea(Shape2D shape) 
    {
        Shape = shape;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
