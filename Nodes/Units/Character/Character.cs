using Godot;
using Godot.Collections;
using System.Collections.Generic;

public enum Type { Unit, Resource, Building }

public partial class Character : CharacterBody2D, IAbilities, IComandQueue
{
    public Character() { }
    public Character(Type type) 
    {
        _type = type;
    }
    enum State { WORK, REST }

    AnimationPlayer _animation;
    Queue<ICommand> _queueCommand = new();
    ICommand _inProcess = null;
    State _state;
    [Export] Type _type;

    [Export] public Player PlayerOwner { get; set; }
    public Type Type { get { return _type; } }
    public AnimationPlayer Animation { get{return _animation;} }
    public ICommand InProcess { get { return _inProcess; } }

    public void AddCommand(ICommand command)
    {
        _queueCommand.Enqueue(command);
    }

    public void InvokeNext() 
    {
        if (_queueCommand.Count > 0 && _inProcess == null) 
        {
            _inProcess = _queueCommand.Dequeue();

            EventHandler handler = ()=>{};
            handler = ()=>
            {
                Animation.Stop();
                Animation.Play("animation/idle");
                _inProcess = null;
            };
            _inProcess.Handler += handler;

            _inProcess.Execute();
            _state = State.WORK;
        }
    }

    public void UndoCommand()
    {
        _inProcess.Undo();
        _inProcess = null;
        _state = State.REST;
    }

    public void SetCommand(ICommand command)
    {
        _queueCommand.Clear();
        if (_inProcess != null) UndoCommand();
        _inProcess = null;
        AddCommand(command);
    }

    public void AddAbility(Node ability)
	{
		AddChild(ability);
    }

	public T GetAbility<T>() where T : Node
	{
        Array<Node> children = GetChildren();
        for (int i = 0; i < children.Count; i++)
        {
            var item = children[i];
            if (item == null) continue;
            if (item is T)
            {
                return item as T;
            }
        }
        return null;
    }

    public override void _Ready()
    {
        _state = State.REST;
        _animation = GetAbility<AnimationPlayer>();
    }
}
