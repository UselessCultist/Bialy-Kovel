using Godot;
using System;
using System.Linq;

public partial class AnimationAbility : AnimationPlayer
{
	Sprite _sprite;
    string _sprite_path;
    AnimationLibrary library = new();

    public AnimationAbility() { }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _sprite = GetParent<Character>().GetAbility<Sprite>();
        _sprite_path = _sprite.GetPath();

        library.AddAnimation("idle", GetIdleAnimation());
        library.AddAnimation("move", GetWalkAnimation());
        library.AddAnimation("attack", GetAttackAnimation());

        AddAnimationLibrary("", library);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    //=============================== Animations ===============================
    public Animation GetIdleAnimation()
    {
        Animation idle = new();
        int trackIndex;

        trackIndex = idle.AddTrack(Animation.TrackType.Value);
        idle.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Capture);
        idle.TrackSetPath(trackIndex, _sprite_path + ":position:x");
        idle.TrackInsertKey(trackIndex, 0.0f, 0);

        trackIndex = idle.AddTrack(Animation.TrackType.Value);
        idle.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Capture);
        idle.TrackSetPath(trackIndex, _sprite_path + ":position:y");
        idle.TrackInsertKey(trackIndex, 0.0f, 0);

        trackIndex = idle.AddTrack(Animation.TrackType.Value);
        idle.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Capture);
        idle.TrackSetPath(trackIndex, _sprite_path + ":rotation");
        idle.TrackInsertKey(trackIndex, 0.0f, 0);

        return idle;
    }

    public Animation GetWalkAnimation()
    {
        Animation move = new();
        move.LoopMode = Animation.LoopModeEnum.Linear;
        int trackIndex;

        trackIndex = move.AddTrack(Animation.TrackType.Value);
        move.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Capture);
        move.TrackSetPath(trackIndex, _sprite_path + ":position:x");
        move.TrackInsertKey(trackIndex, 0.0f, 3);
        move.TrackInsertKey(trackIndex, 0.25f, 0);
        move.TrackInsertKey(trackIndex, 0.5f, -3);
        move.TrackInsertKey(trackIndex, 0.75f, 0);
        move.TrackInsertKey(trackIndex, 1.0f, 3);

        trackIndex = move.AddTrack(Animation.TrackType.Value);
        move.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Capture);
        move.TrackSetPath(trackIndex, _sprite_path + ":position:y");
        move.TrackInsertKey(trackIndex, 0.0f, 0);
        move.TrackInsertKey(trackIndex, 0.25f,-3);
        move.TrackInsertKey(trackIndex, 0.5f, 0);
        move.TrackInsertKey(trackIndex, 0.75f,-3);
        move.TrackInsertKey(trackIndex, 1.0f, 0);

        return move;
    }

    public Animation GetAttackAnimation()
    {
        Animation attack = new();
        int trackIndex;

        trackIndex = attack.AddTrack(Animation.TrackType.Value);
        attack.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Capture);
        attack.TrackSetPath(trackIndex, _sprite_path + ":position:x");
        attack.TrackInsertKey(trackIndex, 0.0f, 0);
        attack.TrackInsertKey(trackIndex, 0.6f, 6);
        attack.TrackInsertKey(trackIndex, 0.7f, 2);
        attack.TrackInsertKey(trackIndex, 0.8f, -6);
        attack.TrackInsertKey(trackIndex, 1.0f, 0);

        trackIndex = attack.AddTrack(Animation.TrackType.Value);
        attack.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Capture);
        attack.TrackSetPath(trackIndex, _sprite_path + ":position:y");
        attack.TrackInsertKey(trackIndex, 0.6f, 0);
        attack.TrackInsertKey(trackIndex, 0.7f, -1.25);
        attack.TrackInsertKey(trackIndex, 0.7f, 0);

        trackIndex = attack.AddTrack(Animation.TrackType.Value);
        attack.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Capture);
        attack.TrackSetPath(trackIndex, _sprite_path + ":rotation");
        attack.TrackInsertKey(trackIndex, 0.0f, 0);
        attack.TrackInsertKey(trackIndex, 0.6f, 0);
        attack.TrackInsertKey(trackIndex, 0.7f, -0.5);
        attack.TrackInsertKey(trackIndex, 0.9f, 0);

        return attack;
    }
}