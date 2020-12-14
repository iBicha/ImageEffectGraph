using UnityEditor.ShaderGraph;

namespace ImageEffectGraph.Editor
{
    [Title("Input", "Image Effects", "Depth Texture")]
    internal class DepthTextureNode : BaseUnityTextureNode
    {
        public DepthTextureNode() : base("_CameraDepthTexture")
        {
            name = "Depth Texture";
        }
    }
}
