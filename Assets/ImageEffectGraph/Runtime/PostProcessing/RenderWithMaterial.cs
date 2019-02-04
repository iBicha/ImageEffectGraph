using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace ImageEffectGraph.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(RenderWithMaterialRenderer), PostProcessEvent.AfterStack, "Custom/Render With Material")]
    public class RenderWithMaterial : PostProcessEffectSettings
    {
        [Tooltip("Material used for rendering.")]
        public MaterialParameter material = new MaterialParameter();

        [Tooltip("Depth texture mode.")]
        public CameraFlagsParameter depthMode = new CameraFlagsParameter() {value = DepthTextureMode.None};
    }

    public class RenderWithMaterialRenderer : PostProcessEffectRenderer<RenderWithMaterial>
    {
        private static readonly int _MainTex = Shader.PropertyToID("_MainTex");

#if UNITY_EDITOR

        private static readonly int _PreviewTexture = Shader.PropertyToID("_PreviewTexture");
        private static readonly int AspectRatio = Shader.PropertyToID("_AspectRatio");
        private static readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");

        private RenderTexture previewRenderTexture;
        private Material aspectBlit;

        public override void Init()
        {
            base.Init();
            //BUG: Can't add more keywords. How to tell shader we're on the stack?
            const string keyword = "UNITY_POST_PROCESSING_STACK_V2";
            Shader.EnableKeyword(keyword);

            previewRenderTexture = new RenderTexture(512, 512, 32, RenderTextureFormat.ARGB32);
            var materialGuid = UnityEditor.AssetDatabase.FindAssets("AspectBlit").FirstOrDefault();
            if (!string.IsNullOrEmpty(materialGuid))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(materialGuid);
                aspectBlit = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            }
        }

        public override void Release()
        {
            base.Release();
            UnityEngine.Object.DestroyImmediate(previewRenderTexture);
        }
#endif

        public override DepthTextureMode GetCameraFlags()
        {
            return settings.depthMode;
        }

        public override void Render(PostProcessRenderContext context)
        {
#if UNITY_EDITOR
            // if (!context.isSceneView)
            if(context.camera.cameraType == CameraType.Game)
            {
//                previewCommandBuffer.Clear();
//                Blit(context.command, context.source, rt, null);
                context.command.Blit(context.source, rt);
//                Graphics.ExecuteCommandBuffer(previewCommandBuffer);
                
//                previewCommandBuffer.Clear();
                context.command.SetGlobalTexture(_PreviewTexture, rt);
//                Graphics.ExecuteCommandBuffer(previewCommandBuffer);                
            }
            else
            {
                context.command.Blit(context.source, context.destination);
                // We don't apply the post processing in Scene View.
                return;
            }            
#endif

            var sampleName = $"RenderWithMaterial({settings.material})";
            context.command.BeginSample(sampleName);
            Blit(context.command, context.source, context.destination, settings.material);
            context.command.EndSample(sampleName);
        }

        private static void Blit(CommandBuffer command, RenderTargetIdentifier source,
            RenderTargetIdentifier destination, Material material, int pass = -1)
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