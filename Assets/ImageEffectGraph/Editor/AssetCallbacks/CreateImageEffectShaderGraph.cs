using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.ShaderGraph;

namespace ImageEffectGraph.Editor
{
    public class CreateImageEffectShaderGraph : EndNameEditAction
    {
        [MenuItem("Assets/Create/Shader/Image Effect Graph", false, 208)]
        public static void CreateMaterialGraph()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateImageEffectShaderGraph>(),
                "New Image Effect Graph.ShaderGraph", null, null);
        }

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var graph = new MaterialGraph();

            var mainTextureNode = new MainTextureNode();
            var mainTextureSamplerNode = new SampleTexture2DNode();
            var masterNode = new ImageEffectMasterNode();

            graph.AddNode(masterNode);
            graph.AddNode(mainTextureSamplerNode);
            graph.AddNode(mainTextureNode);

            graph.Connect(mainTextureNode.GetSlotReference(MainTextureNode.OutputSlotId),
                mainTextureSamplerNode.GetSlotReference(SampleTexture2DNode.TextureInputId));

            graph.Connect(mainTextureSamplerNode.GetSlotReference(SampleTexture2DNode.OutputSlotRGBAId),
                masterNode.GetSlotReference(ImageEffectMasterNode.ColorSlotId));

            var nodes = new AbstractMaterialNode[] {masterNode, mainTextureSamplerNode, mainTextureNode};
          
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var drawState = node.drawState;
                var drawStatePosition = drawState.position;
                drawStatePosition.x -= 300 * i;
                drawState.position = drawStatePosition;
                node.drawState = drawState;
            }

            File.WriteAllText(pathName, EditorJsonUtility.ToJson(graph));
            AssetDatabase.Refresh();
        }
    }
}