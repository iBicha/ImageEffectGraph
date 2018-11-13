using UnityEditor.ShaderGraph;

namespace ImageEffectGraph.Editor
{
    [Title("Input", "Image Effects", "Depth Texture")]
    public class DepthTextureNode : BaseUnityTextureNode
    {
        public DepthTextureNode() : base("_CameraDepthTexture")
        {
            name = "Depth Texture";
        }
    }
}
