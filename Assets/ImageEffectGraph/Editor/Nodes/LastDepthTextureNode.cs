using UnityEditor.ShaderGraph;

namespace ImageEffectGraph.Editor
{
    [Title("Input", "Image Effects", "Last Depth Texture")]
    public class LastDepthTextureNode : BaseUnityTextureNode
    {
        public LastDepthTextureNode() : base("_LastCameraDepthTexture")
        {
            name = "Last Depth Texture";
        }
    }
}

