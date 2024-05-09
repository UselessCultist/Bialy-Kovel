using Godot;
using System;

public partial class Cursor : Node2D
{
    enum State { DEFAULT, BEAM, SET, HIDDEN }

    Node2D _underCursor;
    State _state = State.DEFAULT;
    AnimatedSprite2D arrow;

    public Node2D UnderCursor { get { return _underCursor; } }

    private void _change_state(State new_state)
    {
        switch (new_state)
        {
            case State.DEFAULT:
                arrow.Animation = "default";
                break;
            case State.SET:
                arrow.Animation = "set";
                break;
            case State.BEAM:
                arrow.Animation = "write";
                break;
            case State.HIDDEN:
                arrow.Animation = "hidden";
                break;
        }

        _state = new_state;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        arrow = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Input.MouseMode = Input.MouseModeEnum.Hidden;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        Position = GetGlobalMousePosition();
    }

    public void OnBodyEntered(Node2D unit)
    {
        _underCursor = unit;
    }

    public void OnBodyExited(Node2D area)
    {
        _underCursor = null;
    }
}