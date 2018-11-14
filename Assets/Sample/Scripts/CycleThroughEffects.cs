using System.Collections;
using System.Collections.Generic;
using ImageEffectGraph.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace ImageEffectGraph.Demo
{
    public class CycleThroughEffects : MonoBehaviour
    {
        public float cycleInterval = 1f;
        public Material[] materials;

        private RenderWithMaterial settings;
        private CameraImageEffect cameraImageEffect;
        private int effectIndex = -1;

        private TransitionController transitionController;
        private GUIStyle styleLabel;
        private GUIStyle styleButton;

        void Start()
        {
            var volume = GetComponent<PostProcessVolume>();
            if(volume != null)
            {
                if (!volume.profile.HasSettings<RenderWithMaterial>())
                {
                    Debug.LogWarning("RenderWithMaterial effect not found");
                    Destroy(this);
                    return;
                }
                settings = volume.profile.GetSetting<RenderWithMaterial>();
            }
            
            cameraImageEffect = GetComponent<CameraImageEffect>();
            transitionController = GetComponent<TransitionController>();

            InvokeRepeating("SetNextEffect", 0, cycleInterval);
        }

        private void SetNextEffect()
        {
            effectIndex = (effectIndex + 1) % materials.Length;
            
            if (settings != null)
            {
                settings.material.value = materials[effectIndex];
            }
            else
            {
                cameraImageEffect.material = materials[effectIndex];
            }

            if (materials[effectIndex] == transitionController.material)
            {
                transitionController.StartTransition();
            }
        }

        private void OnGUI()
        {
            //This is not good, but whatever.
            if (styleLabel == null || styleButton == null)
            {
                styleLabel = new GUIStyle(GUI.skin.box) {fontSize = 36};
                styleButton = new GUIStyle(GUI.skin.button) {fontSize = 36};
            }
            
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginHorizontal();
            
            if (effectIndex != -1)
            {
                GUILayout.Label(materials[effectIndex].name, styleLabel);
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Next effect", styleButton))
            {
                //Stop auto cycle if clicked on next
                CancelInvoke();
                SetNextEffect();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}