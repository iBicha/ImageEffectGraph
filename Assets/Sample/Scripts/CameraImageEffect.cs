using UnityEngine;

namespace ImageEffectGraph.Demo
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraImageEffect : MonoBehaviour
    {
        private void OnEnable()
        {
            //If this component is active, we are probably not running post processing stack.
            //TODO: Does this work?
            Shader.DisableKeyword("UNITY_POST_PROCESSING_STACK_V2");
        }

        public Material material;

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (material == null)
            {
                Graphics.Blit(src, dest);
                return;
            }
            Graphics.Blit(src, dest, material);
        }
    }
}
