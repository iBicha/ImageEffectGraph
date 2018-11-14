using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;

namespace ImageEffectGraph.Editor
{
    public abstract class BaseUnityTextureNode : AbstractMaterialNode
    {
        public const int OutputSlotId = 0;

        const string kOutputSlotName = "Out";

        private string unityInputTexture;
        private string unityPreviewInputTexture;

        public BaseUnityTextureNode(string unityInputTexture, string unityPreviewInputTexture = null)
        {
            this.unityInputTexture = unityInputTexture;
            this.unityPreviewInputTexture = unityPreviewInputTexture;
            UpdateNodeAfterDeserialization();
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Texture2DMaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output));
            RemoveSlotsNameNotMatching(new[] { OutputSlotId });
        }

        public override string GetVariableNameForSlot(int slotId)
        {
            if (slotId == OutputSlotId)
                return unityInputTexture;
            
            return base.GetVariableNameForSlot(slotId);
        }
        
        public override void CollectShaderProperties(PropertyCollector properties, GenerationMode generationMode)
        {
            var inputTextureName = unityInputTexture;
            if (generationMode == GenerationMode.Preview && !string.IsNullOrEmpty(unityPreviewInputTexture))
            {
                inputTextureName = unityPreviewInputTexture;
            }
            
            properties.AddShaderProperty(new TextureShaderProperty()
            {
                displayName = "Main Texture",
                overrideReferenceName = inputTextureName,
                generatePropertyBlock = false
            });

        }

        public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
        {
            var inputTextureName = unityInputTexture;
            if (!string.IsNullOrEmpty(unityPreviewInputTexture))
            {
                inputTextureName = unityPreviewInputTexture;
            }

            properties.Add(new PreviewProperty(PropertyType.Texture2D)
            {
                name = inputTextureName
            });
        }

    }
}