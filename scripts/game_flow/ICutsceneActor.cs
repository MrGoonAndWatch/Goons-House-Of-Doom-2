using Godot;
using System;

public abstract partial class ICutsceneActor: CharacterBody3D
{
    public abstract void SetAnimationFlag(string flagName, Variant value);
    public abstract void MoveToPosition(Vector3 position, bool run);
}
