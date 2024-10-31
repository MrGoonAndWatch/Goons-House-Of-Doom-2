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
        //GD.Print($"{sceneName} contains = {sceneName.Contains($"{GameConstants.Mode}/")}");
        if(sceneName.Contains($"{GameConstants.Mode}/"))
            return $"res://scenes/{sceneName}.tscn";
        else
            return $"res://scenes/{GameConstants.Mode}/{sceneName}.tscn";
    }
}
