using Godot;
using System;
using System.Linq;
//================== Pattern Command ==================
// Интерфейс паттерна и важные классы находится в Game.cs
public class CommandExtract : ICommand
{
    public event EventHandler Handler = () => { };
    Character _character;
    Character _target;
    ExtractResource _ability;
    Resource _target_ability;
    public CommandExtract(Character character, Character target)
    {
        _character = character;
        _target = target;
        _ability = _character.GetAbility<ExtractResource>();
        _target_ability = _target.GetAbility<Resource>();
        if (_ability == null) { throw new Exception("This unit can't extract resource"); }
        if (_target_ability == null) { throw new Exception("This object not Resource"); }
    }

    public void Execute()
    {
        Handler += () =>
        {
            _ability.End = () => { };
        };
        _ability.End += Handler;

        _ability.Extract(_target);
    }

    public void Undo()
    {
        _ability.Stop();
    }
}
//=====================================================
public partial class ExtractResource : AbilityWithCommands
{
    enum State { REST, EXTRACT, FOLLOW_TO_STORAGE }

    public EventHandler End = () => { };
    int[] _inventory_resource = new int[(int)ResourceType.MAX];
    const int _inventory_max = 30;
    [Export] int distance_to_interact_with_storage = 8;
    Character _character;
    Character _target = null;
    ResourceType _extract_type = ResourceType.MAX;
    State _state;

    public void ResourceToStorage(Storage storage) 
    {
        for (int i = 0; i < (int)ResourceType.MAX; i++)
        {
            storage.AddToStorage((ResourceType)i, _inventory_resource[i]);
            _inventory_resource[i] = 0;
        }
    }

    private void _change_state(State new_state)
    {
        switch (new_state)
        {
            case State.REST:
                break;
            case State.EXTRACT:
                break;
            case State.FOLLOW_TO_STORAGE:
                break;
        }
        _state = new_state;
    }

    public override void InvokeNext()
    {
        if (_queueCommand.Count > 0 && _inProcess == null)
        {
            _inProcess = _queueCommand.Dequeue();

            EventHandler handler = () => { };
            handler = () =>
            {
                _inProcess = null;
            };
            _inProcess.Handler += handler;

            _inProcess.Execute();
        }
    }

    public void ToStorage()
    {
        MoveAbility ability = _character.GetAbility<MoveAbility>();
        var storage_characters = ability.FindNearestCharactersWithAbility<Storage>(true);
        var storage = ability.FindNearest(storage_characters);

        if (storage == null) 
        {
            Stop();
            return;
        }

        _change_state(State.FOLLOW_TO_STORAGE);
        _target = storage;
        CommandMoveToUnit command = new(_character, _target);
        AddCommand(command);

        EventHandler handler = () => { };
        EventHandler pathComplete = () => { };

        pathComplete = () => 
        {
            Storage storage_ability = _target.GetAbility<Storage>();
            ResourceToStorage(storage_ability);
            ability.PathToTargetComplete -= pathComplete;
        };

        handler = () =>
        {
            _character.Animation.Play("animation/idle");
            _change_state(State.EXTRACT);
            _target = null;
            ability.PathToTargetComplete -= pathComplete;
        };

        command.Handler += handler;
        ability.PathToTargetComplete += pathComplete;

        InvokeNext();
    }

    public void NextExtract()
    {
        MoveAbility ability = _character.GetAbility<MoveAbility>();
        var target = ability.FindNearestResource(_extract_type);

        if (target == null)
        {
            if (_inventory_resource.Sum() != 0)
            {
                ToStorage();
            }
            else 
            {
                Stop();
            }
        }
        else 
        {
            Extract(target);
        }
    }

    public void Extract(Character target) 
    {
        _change_state(State.EXTRACT);
        _target = target;
        Resource ability = target.GetAbility<Resource>();
        _extract_type = ability.ResourceType;

        CommandAttack command = new(_character, target);
        AddCommand(command);
        InvokeNext();
    }

    public void Stop()
    {
        _change_state(State.REST);
        StopCommands();

        _target = null;
        _extract_type = ResourceType.MAX;
        End();
    }

    public override void _Ready()
    {
        _character = GetParent<Character>();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (_state == State.REST)
        {
            return;
        }

        if (_target == null && _state == State.EXTRACT)
        {
            MoveAbility ability = _character.GetAbility<MoveAbility>();

            var resource = ability.FindNearestResource(_extract_type);
            if (resource == null)
            {
                Stop(); return;
            }

            Extract(resource);
            return;
        }

        if (_target != null && _state == State.EXTRACT)
        {
            if (_target.GetAbility<HealthAbility>().IsDead())
            {
                _target = null;
                var res_value = _inventory_resource.Sum() + 10;

                if (res_value >= _inventory_max)
                {
                    res_value = _inventory_max;
                    for (int i = 0; i < _inventory_resource.Length; i++)
                    {
                        if ((int)_extract_type == i)
                        {
                            continue;
                        }
                        res_value -= _inventory_resource[i];
                    }
                    _inventory_resource[(int)_extract_type] = res_value;

                    ToStorage();
                    return;
                }
                else
                {
                    _inventory_resource[(int)_extract_type] = res_value;
                }
                NextExtract();
            }
            return;
        }
    }
}
