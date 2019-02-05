using System;
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

        private string inputTexture;
        public string previewInputTexture;

        public BaseUnityTextureNode(string inputTexture, string previewInputTexture = null)
        {
            this.inputTexture = inputTexture;
            this.previewInputTexture = previewInputTexture;
            UpdateNodeAfterDeserialization();
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Texture2DMaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output));
            RemoveSlotsNameNotMatching(new[] {OutputSlotId});
        }

        public override string GetVariableNameForSlot(int slotId)
        {
            if (slotId == OutputSlotId)
                return inputTexture;

            return base.GetVariableNameForSlot(slotId);
        }

        public override void CollectShaderProperties(PropertyCollector properties, GenerationMode generationMode)
        {
            base.CollectShaderProperties(properties, generationMode);
            properties.AddShaderProperty(new TextureShaderProperty()
            {
                displayName = "Main Texture",
                overrideReferenceName = inputTexture,
                generatePropertyBlock = false
            });

            if (generationMode.IsPreview() && !string.IsNullOrEmpty(previewInputTexture))
            {
                properties.AddShaderProperty(new TextureShaderProperty()
                {
                    displayName = "Preview Texture",
                    overrideReferenceName = previewInputTexture,
                    generatePropertyBlock = false
                });
            }
        }

        public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
        {
            base.CollectPreviewMaterialProperties(properties);
            properties.Add(new PreviewProperty(PropertyType.Texture2D)
            {
                name = inputTexture,
                textureValue = Shader.GetGlobalTexture(previewInputTexture)
            });
        }
    }
}