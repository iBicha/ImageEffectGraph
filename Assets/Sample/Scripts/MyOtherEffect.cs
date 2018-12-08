using System;
using ImageEffectGraph.PostProcessing;
using UnityEngine.Rendering.PostProcessing;

namespace ImageEffectGraph.Demo
{
    [Serializable]
    [PostProcess(typeof(RenderWithMaterialRenderer), PostProcessEvent.AfterStack, "Custom/My Other Effect")]
    public class MyOtherEffect : RenderWithMaterial
    {
    }
}
