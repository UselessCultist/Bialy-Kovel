using Godot;
using System;
using System.Collections.Generic;

public delegate void EventHandler();

//================== Pattern Command ==================
// Интерфейс паттерна и важные классы находится в Game.cs
public interface ICommand
{
    event EventHandler Handler;
    void Execute();
    void Undo();
}
//=================== Command Queue ===================
public interface IComandQueue 
{
    public ICommand InProcess { get; }

    public void AddCommand(ICommand command);

    public void InvokeNext();

    public void UndoCommand();

    public void SetCommand(ICommand command);
}

//============= GAS (Game Ability System) =============
public interface IAbilities
{
    public void AddAbility(Node ability);
    public T GetAbility<T>() where T : Node;
}

//============ AWC (Ability With Commands) ============
public partial class AbilityWithCommands : Node, IComandQueue
{
    protected AnimationPlayer _animation;
    protected Queue<ICommand> _queueCommand = new();
    protected ICommand _inProcess = null;

    public virtual ICommand InProcess { get { return _inProcess; } }

    public virtual void AddCommand(ICommand command)
    {
        _queueCommand.Enqueue(command);
    }

    public virtual void InvokeNext()
    {
        if (_queueCommand.Count > 0 && _inProcess == null)
        {
            _inProcess = _queueCommand.Dequeue();

            EventHandler handler = () => { };
            handler = () =>
            {
                _inProcess.Undo();
                _inProcess = null;
            };
            _inProcess.Handler += handler;

            _inProcess.Execute();
        }
    }

    public virtual void UndoCommand()
    {
        _inProcess.Undo();
    }

    public virtual void SetCommand(ICommand command)
    {
        _queueCommand.Clear();
        AddCommand(command);
    }
}
//=====================================================

public partial class Game : Node
{
    [Signal] public delegate void GameReadyEventHandler();

    TileMapAstar2D _tilemap;
    public TileMapAstar2D TileMap { get { return _tilemap; } }

    Player GetPlayerByID(int ID) 
    {
        foreach (var child in GetChildren())
        {
            if (child is Player player)
            {
                if (player.ID == ID)
                {
                    return player;
                }
            }
        }
        return null;
    }

    public T GetFirstNode<T>(Node parent) where T : Node
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is T)
            {
                return child as T;
            }
        }
        return null;
    }

    static public List<T> GetListNode<T>(Node parent) where T : Node
    {
        List<T> list = new();
        foreach (var child in parent.GetChildren())
        {
            if (child is T Tchild)
            {
                list.Add(Tchild);
            }
        }
        return list;
    }

    public void Initializate()
    {
        _tilemap = GetFirstNode<TileMapAstar2D>(this);
        EmitSignal(SignalName.GameReady);
    }

    public override void _Ready()
    {
        Initializate();
        Player player = GetPlayerByID(1);
        Player computer = new(-1, PlayerState.Computer);
        AddChild(computer);

        for (int i = 0; i < 1; i++)
        {
            AddChild(Factory.CreateWorker(player, new(i,1)));
        }

        AddChild(Factory.CreateStone(new(-10, -10)));
        AddChild(Factory.CreateStone(new(5, 5)));
        AddChild(Factory.CreateStone(new(6, 2)));
        AddChild(Factory.CreateStone(new(10, 20)));
        AddChild(Factory.CreateStorage(player, new(10,5)));
        AddChild(Factory.CreateStorage(player, new(-10, 10)));
    }
}