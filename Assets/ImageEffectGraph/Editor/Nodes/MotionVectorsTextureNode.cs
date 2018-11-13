using UnityEditor.ShaderGraph;

namespace ImageEffectGraph.Editor
{
    [Title("Input", "Image Effects", "Motion Vectors Texture")]
    public class MotionVectorsTextureNode : BaseUnityTextureNode
    {
        public MotionVectorsTextureNode() : base("_CameraMotionVectorsTexture")
        {
            name = "Motion Vectors Texture";
        }
    }
}
