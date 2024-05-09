using Godot;
using System;

public enum ResourceType { Wood, Stone, MAX }

public partial class Resource : Node
{
    public Resource(ResourceType type) 
    {
        _resourceType = type;
    }

    ResourceType _resourceType;
    public ResourceType ResourceType { get { return _resourceType; } }
}
