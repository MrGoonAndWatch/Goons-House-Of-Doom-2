using Godot;

public partial class LoadGammaSettings : ColorRect
{
    public override void _Ready()
    {
        if(HasNode(new NodePath(GameConstants.NodePaths.FromSceneRoot.GammaCorrectionSolo)))
        {
            var gammaRect = GetNode<CanvasItem>(GameConstants.NodePaths.FromSceneRoot.GammaCorrectionSolo);
            var gammaShader = gammaRect.Material as ShaderMaterial;
            var globalSettings = DataSaver.GetGlobalSettings();
            gammaShader.SetShaderParameter(GameConstants.ShaderParameters.Gamma, globalSettings.Gamma);
        }
    }
}
