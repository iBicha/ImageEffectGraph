using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Object = UnityEngine.Object;

namespace ImageEffectGraph.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(RenderWithMaterialRenderer), PostProcessEvent.AfterStack, "Custom/Render With Material")]
    public class RenderWithMaterial : PostProcessEffectSettings
    {
        [Tooltip("Material used for rendering.")]
        public MaterialParameter material = new MaterialParameter();
    
        [Tooltip("Depth texture mode.")]
        public CameraFlagsParameter depthMode = new CameraFlagsParameter(){value = DepthTextureMode.None};

    }

    public class RenderWithMaterialRenderer : PostProcessEffectRenderer<RenderWithMaterial>
    {
        private static int _MainTex = Shader.PropertyToID("_MainTex");
        private static int _PreviewTexture = Shader.PropertyToID("_PreviewTexture");

        public override DepthTextureMode GetCameraFlags()
        {
            return settings.depthMode;
        }

        public override void Render(PostProcessRenderContext context)
        {
#if UNITY_EDITOR
            if (!context.isSceneView)
            {
                context.command.SetGlobalTexture(_PreviewTexture, context.source);
            }
#endif
        
            var sampleName = $"RenderWithMaterial({settings.material})";
            context.command.BeginSample(sampleName);
            Blit(context.command, context.source, context.destination, settings.material);
            context.command.EndSample(sampleName);
        }


        private static void Blit(CommandBuffer command, RenderTargetIdentifier source, RenderTargetIdentifier destination,
            Material material, int pass = -1)
        {
            if (material == null)
            {
                command.BuiltinBlit(source, destination);
                return;
            }

            command.SetGlobalTexture(_MainTex, source);
            command.SetRenderTargetWithLoadStoreAction(destination, RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store);

            command.DrawMesh(RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, material, 0, pass);
        }
    } 

}

