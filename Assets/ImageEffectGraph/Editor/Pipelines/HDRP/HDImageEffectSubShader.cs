﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory.Ast;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace ImageEffectGraph.Editor.HDPipeline
{
    [Serializable]
    internal class HDImageEffectSubShader : IImageEffectSubShader
    {
        static readonly NeededCoordinateSpace k_PixelCoordinateSpace = NeededCoordinateSpace.None;

        struct Pass
        {
            public string Name;
            public List<int> VertexShaderSlots;
            public List<int> PixelShaderSlots;
        }

        //TODO
        Pass m_ImageEffectPass = new Pass
        {
            Name = "Pass",
            PixelShaderSlots = new List<int>
            {
                ImageEffectMasterNode.ColorSlotId,
            },
            VertexShaderSlots = new List<int>()
            {
                
            }
        };

        public string GetSubshader(IMasterNode masterNode, GenerationMode mode, List<string> sourceAssetDependencyPaths = null)
        {
            //TODO:
//            if (sourceAssetDependencyPaths != null)
//            {
//                // LightWeightUnlitSubShader.cs
//                sourceAssetDependencyPaths.Add(AssetDatabase.GUIDToAssetPath("3ef30c5c1d5fc412f88511ef5818b654"));
//            }

            var templatePath = GetTemplatePath("hdEffectPass.template");
//            var extraPassesTemplatePath = GetTemplatePath("lightweightUnlitExtraPasses.template");
            if (!File.Exists(templatePath) /*|| !File.Exists(extraPassesTemplatePath)*/)
                return string.Empty;

            if (sourceAssetDependencyPaths != null)
            {
                sourceAssetDependencyPaths.Add(templatePath);
//                sourceAssetDependencyPaths.Add(extraPassesTemplatePath);
            }

            string forwardTemplate = File.ReadAllText(templatePath);
//            string extraTemplate = File.ReadAllText(extraPassesTemplatePath);

            var imageEffectMasterNode = masterNode as ImageEffectMasterNode;
            var pass = m_ImageEffectPass;
            var subShader = new ShaderStringBuilder();
            subShader.AppendLine("SubShader");
            using (subShader.BlockScope())
            {
                subShader.AppendLine("Tags{ \"RenderPipeline\" = \"HDRenderPipeline\" \"PreviewType\" = \"Plane\"}");

//                var materialTags = ShaderGenerator.BuildMaterialTags(imageEffectMasterNode.surfaceType);
//                var tagsBuilder = new ShaderStringBuilder(0);
//                materialTags.GetTags(tagsBuilder);
//                subShader.AppendLines(tagsBuilder.ToString());

                var materialOptions = ShaderGenerator.GetMaterialOptions(imageEffectMasterNode.surfaceType, imageEffectMasterNode.alphaMode, false);
                subShader.AppendLines(GetShaderPassFromTemplate(
                        forwardTemplate,
                        imageEffectMasterNode,
                        pass,
                        mode,
                        materialOptions));
            }

            return subShader.ToString();
        }

        public bool IsPipelineCompatible(RenderPipelineAsset renderPipelineAsset)
        {
            const string RenderPipelineAssetName = "HDRenderPipelineAsset";
            
            if (renderPipelineAsset == null)
                return false;

            var rpType = renderPipelineAsset.GetType();
            return RuntimeUtilities.GetAllAssemblyTypes()
                .Where(t => t.Name == RenderPipelineAssetName)
                .Any(t => t.IsAssignableFrom(rpType));
        }

        static string GetTemplatePath(string templateName)
        {
            var pathSegments = new[] { "Assets", "ImageEffectGraph", "Editor", "Pipelines", "HDRP", templateName };
            var path = pathSegments.Aggregate("", Path.Combine);
            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format(@"Cannot find a template with name ""{0}"".", templateName));
            return path;
        }

        static string GetShaderPassFromTemplate(string template, ImageEffectMasterNode masterNode, Pass pass, GenerationMode mode, SurfaceMaterialOptions materialOptions)
        {
            // ----------------------------------------------------- //
            //                         SETUP                         //
            // ----------------------------------------------------- //

            // -------------------------------------
            // String builders

            var shaderProperties = new PropertyCollector();
            var functionBuilder = new ShaderStringBuilder(1);
            var functionRegistry = new FunctionRegistry(functionBuilder);

            var defines = new ShaderStringBuilder(1);
            var graph = new ShaderStringBuilder(0);

            var vertexDescriptionInputStruct = new ShaderStringBuilder(1);
            var vertexDescriptionStruct = new ShaderStringBuilder(1);
            var vertexDescriptionFunction = new ShaderStringBuilder(1);

            var surfaceDescriptionInputStruct = new ShaderStringBuilder(1);
            var surfaceDescriptionStruct = new ShaderStringBuilder(1);
            var surfaceDescriptionFunction = new ShaderStringBuilder(1);

            var vertexInputStruct = new ShaderStringBuilder(1);
            var vertexOutputStruct = new ShaderStringBuilder(2);

            var vertexShader = new ShaderStringBuilder(2);
            var vertexShaderDescriptionInputs = new ShaderStringBuilder(2);
            var vertexShaderOutputs = new ShaderStringBuilder(2);

            var pixelShader = new ShaderStringBuilder(2);
            var pixelShaderSurfaceInputs = new ShaderStringBuilder(2);
            var pixelShaderSurfaceRemap = new ShaderStringBuilder(2);

            // -------------------------------------
            // Get Slot and Node lists per stage

            var vertexSlots = pass.VertexShaderSlots.Select(masterNode.FindSlot<MaterialSlot>).ToList();
            var vertexNodes = ListPool<AbstractMaterialNode>.Get();
            NodeUtils.DepthFirstCollectNodesFromNode(vertexNodes, masterNode, NodeUtils.IncludeSelf.Include, pass.VertexShaderSlots);

            var pixelSlots = pass.PixelShaderSlots.Select(masterNode.FindSlot<MaterialSlot>).ToList();
            var pixelNodes = ListPool<AbstractMaterialNode>.Get();
            NodeUtils.DepthFirstCollectNodesFromNode(pixelNodes, masterNode, NodeUtils.IncludeSelf.Include, pass.PixelShaderSlots);

            // -------------------------------------
            // Get Requirements

            var vertexRequirements = ShaderGraphRequirements.FromNodes(vertexNodes, ShaderStageCapability.Vertex, false);
            var pixelRequirements = ShaderGraphRequirements.FromNodes(pixelNodes, ShaderStageCapability.Fragment);
            var graphRequirements = pixelRequirements.Union(vertexRequirements);
            var surfaceRequirements = ShaderGraphRequirements.FromNodes(pixelNodes, ShaderStageCapability.Fragment, false);

            var modelRequiements = ShaderGraphRequirements.none;
            modelRequiements.requiresNormal |= k_PixelCoordinateSpace;
            modelRequiements.requiresTangent |= k_PixelCoordinateSpace;
            modelRequiements.requiresBitangent |= k_PixelCoordinateSpace;
            modelRequiements.requiresPosition |= k_PixelCoordinateSpace;
            modelRequiements.requiresViewDir |= k_PixelCoordinateSpace;
            modelRequiements.requiresMeshUVs.Add(UVChannel.UV0);
            modelRequiements.requiresMeshUVs.Add(UVChannel.UV1);

            // ----------------------------------------------------- //
            //                START SHADER GENERATION                //
            // ----------------------------------------------------- //

            // -------------------------------------
            // Calculate material options

            var blendingBuilder = new ShaderStringBuilder(1);
            var cullingBuilder = new ShaderStringBuilder(1);
            var zTestBuilder = new ShaderStringBuilder(1);
            var zWriteBuilder = new ShaderStringBuilder(1);

            // Cull Off ZWrite Off ZTest Always
            materialOptions.cullMode = SurfaceMaterialOptions.CullMode.Off;
            materialOptions.zWrite = SurfaceMaterialOptions.ZWrite.Off;
            materialOptions.zTest = SurfaceMaterialOptions.ZTest.Always;
            
            materialOptions.GetBlend(blendingBuilder);
            materialOptions.GetCull(cullingBuilder);
            materialOptions.GetDepthTest(zTestBuilder);
            materialOptions.GetDepthWrite(zWriteBuilder);

            // -------------------------------------
            // Generate defines

            //TODO
//            if (masterNode.IsSlotConnected(UnlitMasterNode.AlphaThresholdSlotId))
//                defines.AppendLine("#define _AlphaClip 1");

            if (masterNode.surfaceType == SurfaceType.Transparent && masterNode.alphaMode == AlphaMode.Premultiply)
                defines.AppendLine("#define _ALPHAPREMULTIPLY_ON 1");

            if (graphRequirements.requiresDepthTexture)
                defines.AppendLine("#define REQUIRE_DEPTH_TEXTURE");

            if (graphRequirements.requiresCameraOpaqueTexture)
                defines.AppendLine("#define REQUIRE_OPAQUE_TEXTURE");

            // ----------------------------------------------------- //
            //                START VERTEX DESCRIPTION               //
            // ----------------------------------------------------- //

            // -------------------------------------
            // Generate Input structure for Vertex Description function
            // TODO - Vertex Description Input requirements are needed to exclude intermediate translation spaces

            vertexDescriptionInputStruct.AppendLine("struct VertexDescriptionInputs");
            using (vertexDescriptionInputStruct.BlockSemicolonScope())
            {
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(vertexRequirements.requiresNormal, InterpolatorType.Normal, vertexDescriptionInputStruct);
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(vertexRequirements.requiresTangent, InterpolatorType.Tangent, vertexDescriptionInputStruct);
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(vertexRequirements.requiresBitangent, InterpolatorType.BiTangent, vertexDescriptionInputStruct);
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(vertexRequirements.requiresViewDir, InterpolatorType.ViewDirection, vertexDescriptionInputStruct);
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(vertexRequirements.requiresPosition, InterpolatorType.Position, vertexDescriptionInputStruct);

                if (vertexRequirements.requiresVertexColor)
                    vertexDescriptionInputStruct.AppendLine("float4 {0};", ShaderGeneratorNames.VertexColor);

                if (vertexRequirements.requiresScreenPosition)
                    vertexDescriptionInputStruct.AppendLine("float4 {0};", ShaderGeneratorNames.ScreenPosition);

                foreach (var channel in vertexRequirements.requiresMeshUVs.Distinct())
                    vertexDescriptionInputStruct.AppendLine("half4 {0};", channel.GetUVName());
            }

            // -------------------------------------
            // Generate Output structure for Vertex Description function

            GraphUtil.GenerateVertexDescriptionStruct(vertexDescriptionStruct, vertexSlots);

            // -------------------------------------
            // Generate Vertex Description function

            GraphUtil.GenerateVertexDescriptionFunction(
                masterNode.owner,
                vertexDescriptionFunction,
                functionRegistry,
                shaderProperties,
                mode,
                vertexNodes,
                vertexSlots);

            // ----------------------------------------------------- //
            //               START SURFACE DESCRIPTION               //
            // ----------------------------------------------------- //

            // -------------------------------------
            // Generate Input structure for Surface Description function
            // Surface Description Input requirements are needed to exclude intermediate translation spaces

            surfaceDescriptionInputStruct.AppendLine("struct SurfaceDescriptionInputs");
            using (surfaceDescriptionInputStruct.BlockSemicolonScope())
            {
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(surfaceRequirements.requiresNormal, InterpolatorType.Normal, surfaceDescriptionInputStruct);
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(surfaceRequirements.requiresTangent, InterpolatorType.Tangent, surfaceDescriptionInputStruct);
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(surfaceRequirements.requiresBitangent, InterpolatorType.BiTangent, surfaceDescriptionInputStruct);
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(surfaceRequirements.requiresViewDir, InterpolatorType.ViewDirection, surfaceDescriptionInputStruct);
                ShaderGenerator.GenerateSpaceTranslationSurfaceInputs(surfaceRequirements.requiresPosition, InterpolatorType.Position, surfaceDescriptionInputStruct);

                if (surfaceRequirements.requiresVertexColor)
                    surfaceDescriptionInputStruct.AppendLine("float4 {0};", ShaderGeneratorNames.VertexColor);

                if (surfaceRequirements.requiresScreenPosition)
                    surfaceDescriptionInputStruct.AppendLine("float4 {0};", ShaderGeneratorNames.ScreenPosition);

                if (surfaceRequirements.requiresFaceSign)
                    surfaceDescriptionInputStruct.AppendLine("float {0};", ShaderGeneratorNames.FaceSign);

                foreach (var channel in surfaceRequirements.requiresMeshUVs.Distinct())
                    surfaceDescriptionInputStruct.AppendLine("half4 {0};", channel.GetUVName());
            }

            // -------------------------------------
            // Generate Output structure for Surface Description function

            GraphUtil.GenerateSurfaceDescriptionStruct(surfaceDescriptionStruct, pixelSlots, true);

            // -------------------------------------
            // Generate Surface Description function

            GraphUtil.GenerateSurfaceDescriptionFunction(
                pixelNodes,
                masterNode,
                masterNode.owner,
                surfaceDescriptionFunction,
                functionRegistry,
                shaderProperties,
                pixelRequirements,
                mode,
                "PopulateSurfaceData",
                "SurfaceDescription",
                null,
                pixelSlots);

            // ----------------------------------------------------- //
            //           GENERATE VERTEX > PIXEL PIPELINE            //
            // ----------------------------------------------------- //

            // -------------------------------------
            // Generate Input structure for Vertex shader

            GraphUtil.GenerateApplicationVertexInputs(vertexRequirements.Union(pixelRequirements.Union(modelRequiements)), vertexInputStruct);

            // -------------------------------------
            // Generate standard transformations
            // This method ensures all required transform data is available in vertex and pixel stages

            ShaderGenerator.GenerateStandardTransforms(
                3,
                10,
                vertexOutputStruct,
                vertexShader,
                vertexShaderDescriptionInputs,
                vertexShaderOutputs,
                pixelShader,
                pixelShaderSurfaceInputs,
                pixelRequirements,
                surfaceRequirements,
                modelRequiements,
                vertexRequirements,
                CoordinateSpace.World);

            // -------------------------------------
            // Generate pixel shader surface remap

            foreach (var slot in pixelSlots)
            {
                pixelShaderSurfaceRemap.AppendLine("{0} = surf.{0};", slot.shaderOutputName);
            }

            // -------------------------------------
            // Extra pixel shader work

            var faceSign = new ShaderStringBuilder();

            if (pixelRequirements.requiresFaceSign)
                faceSign.AppendLine(", half FaceSign : VFACE");

            // ----------------------------------------------------- //
            //                      FINALIZE                         //
            // ----------------------------------------------------- //

            // -------------------------------------
            // Combine Graph sections

            graph.AppendLine(shaderProperties.GetPropertiesDeclaration(1));

            graph.AppendLine(vertexDescriptionInputStruct.ToString());
            graph.AppendLine(surfaceDescriptionInputStruct.ToString());

            graph.AppendLine(functionBuilder.ToString());

            graph.AppendLine(vertexDescriptionStruct.ToString());
            graph.AppendLine(vertexDescriptionFunction.ToString());

            graph.AppendLine(surfaceDescriptionStruct.ToString());
            graph.AppendLine(surfaceDescriptionFunction.ToString());

            graph.AppendLine(vertexInputStruct.ToString());

            // -------------------------------------
            // Generate final subshader

            var resultPass = template.Replace("${Tags}", string.Empty);
            resultPass = resultPass.Replace("${Blending}", blendingBuilder.ToString());
            resultPass = resultPass.Replace("${Culling}", cullingBuilder.ToString());
            resultPass = resultPass.Replace("${ZTest}", zTestBuilder.ToString());
            resultPass = resultPass.Replace("${ZWrite}", zWriteBuilder.ToString());
            resultPass = resultPass.Replace("${Defines}", defines.ToString());

            resultPass = resultPass.Replace("${Graph}", graph.ToString());
            resultPass = resultPass.Replace("${VertexOutputStruct}", vertexOutputStruct.ToString());

            resultPass = resultPass.Replace("${VertexShader}", vertexShader.ToString());
            resultPass = resultPass.Replace("${VertexShaderDescriptionInputs}", vertexShaderDescriptionInputs.ToString());
            resultPass = resultPass.Replace("${VertexShaderOutputs}", vertexShaderOutputs.ToString());

            resultPass = resultPass.Replace("${FaceSign}", faceSign.ToString());
            resultPass = resultPass.Replace("${PixelShader}", pixelShader.ToString());
            resultPass = resultPass.Replace("${PixelShaderSurfaceInputs}", pixelShaderSurfaceInputs.ToString());
            resultPass = resultPass.Replace("${PixelShaderSurfaceRemap}", pixelShaderSurfaceRemap.ToString());

            return resultPass;
        }
    }
}
