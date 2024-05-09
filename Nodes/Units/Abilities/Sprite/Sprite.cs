using Godot;
using System;

public partial class Sprite : Sprite2D
{

	public Sprite() 
	{
		Image image = Image.LoadFromFile("res://Texture/Units/Worker/Human.png");
		Texture2D texture = ImageTexture.CreateFromImage(image);
		Texture = texture;
	}

    public Sprite(Image image)
    {
        Texture2D texture = ImageTexture.CreateFromImage(image);
        Texture = texture;
    }

    public Sprite(Texture2D texture)
    {
        Texture = texture;
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
