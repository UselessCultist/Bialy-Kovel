using Godot;

public enum ResourceType { Wood, Stone, MAX }

public partial class Resource : Node
{
    [Export]
    private ResourceType _resourceType;

    public Resource(){}
    public Resource(ResourceType type)
    {
        _resourceType = type;
    }
    public ResourceType ResourceType { get { return _resourceType; } }
}
