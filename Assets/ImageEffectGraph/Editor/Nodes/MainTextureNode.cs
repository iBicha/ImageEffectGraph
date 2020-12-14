using UnityEditor.ShaderGraph;

namespace ImageEffectGraph.Editor
{
    [Title("Input", "Image Effects", "Main Texture")]
    internal class MainTextureNode : BaseUnityTextureNode
    {
        public MainTextureNode() : base("_MainTex", "_PreviewTexture")
        {
            name = "Main Texture (Input)";
        }
    }
}