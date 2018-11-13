using UnityEditor.ShaderGraph;

namespace ImageEffectGraph.Editor
{
    [Title("Input", "Image Effects", "Main Texture")]
    public class MainTextureNode : BaseUnityTextureNode
    {
        public MainTextureNode() : base("_MainTex")
        {
            name = "Main Texture (Input)";
        }
    }
}