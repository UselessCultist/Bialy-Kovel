using Godot;
using System;

public partial class Camera : Camera2D
{
    Vector2 _velocity = Vector2.Zero;
    float _speed = 1000;

    public override void _PhysicsProcess(double delta)
    {
        _velocity.X = Input.GetAxis("ui_left", "ui_right");
        _velocity.Y = Input.GetAxis("ui_up", "ui_down");

        _velocity *= (float)(_speed * delta);
        GlobalPosition += _velocity;
    }
}
