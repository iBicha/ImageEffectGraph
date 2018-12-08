using ImageEffectGraph.Demo;
using ImageEffectGraph.Editor.PostProcessing;
using UnityEditor.Rendering.PostProcessing;

namespace ImageEffectGraph.Editor.Demo
{
    [PostProcessEditor(typeof(MyOtherEffect))]
    public class MyOtherEffectEditor : RenderWithMaterialEditor
    {
    }
}
