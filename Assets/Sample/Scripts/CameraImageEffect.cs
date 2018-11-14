using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImageEffectGraph.Demo
{
    [ExecuteInEditMode]
    public class CameraImageEffect : MonoBehaviour
    {
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
