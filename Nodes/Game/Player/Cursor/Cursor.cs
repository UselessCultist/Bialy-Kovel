using Godot;
using System;
using System.Collections.Generic;

public partial class Cursor : Area2D
{
    enum State { DEFAULT, BEAM, SET, HIDDEN }

    bool dragging = false;
    Vector2 drag_start;
    RectangleShape2D _selection_shape = new();
    CollisionShape2D _selection;

    Player player;
    State _state = State.DEFAULT;
    AnimatedSprite2D arrow;

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

    public override void _Input(InputEvent @event)
    {


        if (@event is InputEventMouseButton mouse_event && mouse_event.ButtonIndex == MouseButton.Left)
        {
            // When the mouse button is pressed, then the dragging starts
            if (mouse_event.Pressed && !dragging)
            {
                foreach (var unit in player._selectedUnits)
                {
                    unit.GetAbility<SelectArea>().deselect();
                }
                player._selectedUnits = new();
                dragging = true;
                drag_start = GetGlobalMousePosition();

                return;
            }

            // If I'm already dragging and release mouse button
            if (!mouse_event.Pressed && dragging)
            {
                QueueRedraw();
                dragging = false;
                Vector2 drag_end = GetGlobalMousePosition();

                _selection_shape.Size = (drag_end - drag_start).Abs();
                _selection.GlobalPosition = (drag_end + drag_start)/2;

                foreach (var area in GetOverlappingAreas())
                {
                    Character unit = area.GetParent<Character>();
                    unit.GetAbility<SelectArea>().select();
                    player._selectedUnits.Add(unit);
                }

                _selection_shape.Size = new(0,0);
                _selection.GlobalPosition = new(0,0);
            }
        }
    }

    public Character GetUnitUnderCursor() 
    {
        _selection_shape.Size = new(2,2);
        _selection.GlobalPosition = GetGlobalMousePosition();
        foreach (var area in GetOverlappingAreas())
        {
            Character unit = area.GetParent<Character>();
            return unit;
        }
        return null;
    }

    public override void _Draw()
    {
        if (dragging)
        {
            Vector2 drag_end = GetGlobalMousePosition();
            var start = ToLocal(drag_start);
            var end = ToLocal(drag_end);
            var center = (start + end) / 2;
            var size = (end - start).Abs();

            DrawRect(new Rect2(start, size), Color.Color8(0, 0, 255, 200));
            DrawCircle(center, 5, Color.Color8(255, 0, 0, 255));
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _selection = GetNode<CollisionShape2D>("CollisionShape2D");
        _selection.Shape = _selection_shape;

        player = GetParent<Player>();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (dragging)
        {
            Vector2 drag_end = GetGlobalMousePosition();
            GD.Print(_selection_shape.Size);
            _selection_shape.Size = (drag_end - drag_start).Abs();
            _selection.GlobalPosition = (drag_end + drag_start) / 2;

            QueueRedraw();
        }
    }
}