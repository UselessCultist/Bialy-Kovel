using Godot;
using System;

public partial class Storage : Node
{
    public void AddToStorage(ResourceType resource, int value) 
    {
        Character unit = GetParent<Character>();
        unit.PlayerOwner.AddResource(resource, value);
    }
}
