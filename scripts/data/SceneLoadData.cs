using Godot;

public partial class SceneLoadData : GodotObject
{
    public string TargetScene;
    public Vector3 LoadPosition;
    public Vector3 LoadRotation;

    public string GetTargetSceneFullPath()
    {
        return $"res://scenes/{TargetScene}.tscn";
    }
}
