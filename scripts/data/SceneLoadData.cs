using Godot;

public partial class SceneLoadData : GodotObject
{
    public string TargetScene;
    public Vector3 LoadPosition;
    public Vector3 LoadRotation;

    public string GetTargetSceneFullPath()
    {
        return GetSceneFullPath(TargetScene);
    }

    public static string GetSceneFullPath(string sceneName)
    {
        return $"res://scenes/{sceneName}.tscn";
    }
}
