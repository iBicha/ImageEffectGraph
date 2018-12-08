using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImageEffectGraph.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor;
using UnityEditor.Rendering.PostProcessing;

namespace ImageEffectGraph.Editor.PostProcessing
{
    [PostProcessEditor(typeof(RenderWithMaterial))]
    public class RenderWithMaterialEditor : PostProcessEffectEditor<RenderWithMaterial>
    {
        List<SerializedParameterOverride> m_Parameters;

        protected PostProcessEffectSettings target { get; private set; }
        protected SerializedObject serializedObject { get; private set; }

        private MaterialEditor materialEditor;
        private UnityEditor.Editor cachedEditor;
        private Material cachedMaterial;
        private MaterialProperty[] properties;

        public override void OnEnable()
        {
            InitInternals();
            FindProperties();
        }

        public override void OnInspectorGUI()
        {
            //Show material
            OnInspectorGuiDefault();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            //Show material properties
            var settings = (RenderWithMaterial) target;
            var guiEnabled = GUI.enabled;
            GUI.enabled = settings.material.overrideState;
            MaterialPropertiesGui(settings.material.value);
            GUI.enabled = guiEnabled;

            //TODO: figure out how to duplicate same effect, and get them to render as well
            //https://github.com/iBicha/ImageEffectGraph/issues/7
            /*
            if (GUILayout.Button("Create another effect", EditorStyles.miniButton))
            {
                CreateNewRenderWithMaterialEffect();
            }
            */
        }

        private void CreateNewRenderWithMaterialEffect()
        {
            var effectListEditor = FindEffectListContainingThisEditor();
            if (effectListEditor != null)
            {
                var tyEffectListEditor = typeof(EffectListEditor);
                var MIAddEffectOverride =
                    tyEffectListEditor.GetMethod("AddEffectOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                MIAddEffectOverride.Invoke(effectListEditor, new object[] {typeof(RenderWithMaterial)});
            }
        }

        private EffectListEditor FindEffectListContainingThisEditor()
        {
            var tyEffectListEditor = typeof(EffectListEditor);
            var tyPostProcessVolumeEditor =
                tyEffectListEditor.Assembly.GetType(
                    "UnityEditor.Rendering.PostProcessing.PostProcessVolumeEditor");

            var tyPostProcessProfileEditor =
                tyEffectListEditor.Assembly.GetType(
                    "UnityEditor.Rendering.PostProcessing.PostProcessProfileEditor");

            var volumeEditors = Resources.FindObjectsOfTypeAll(tyPostProcessVolumeEditor);
            var profileEditors = Resources.FindObjectsOfTypeAll(tyPostProcessProfileEditor);

            List<EffectListEditor> effectListEditors = new List<EffectListEditor>();

            if (volumeEditors.Length > 0)
            {
                var FIm_EffectList =
                    tyPostProcessVolumeEditor.GetField("m_EffectList", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var volumeEditor in volumeEditors)
                {
                    var effectListEditor = (EffectListEditor) FIm_EffectList.GetValue(volumeEditor);
                    if (effectListEditor != null)
                    {
                        effectListEditors.Add(effectListEditor);
                    }
                }
            }

            if (profileEditors.Length > 0)
            {
                var FIm_EffectList =
                    tyPostProcessProfileEditor.GetField("m_EffectList", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var profileEditor in profileEditors)
                {
                    var effectListEditor = (EffectListEditor) FIm_EffectList.GetValue(profileEditor);
                    if (effectListEditor != null)
                    {
                        effectListEditors.Add(effectListEditor);
                    }
                }
            }

            if (effectListEditors.Count > 0)
            {
                var FIm_Editors =
                    tyEffectListEditor.GetField("m_Editors", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var effectListEditor in effectListEditors)
                {
                    List<PostProcessEffectBaseEditor> m_Editors =
                        (List<PostProcessEffectBaseEditor>) FIm_Editors.GetValue(effectListEditor);
                    if (m_Editors != null)
                    {
                        if (m_Editors.Contains(this))
                        {
                            return effectListEditor;
                        }
                    }
                }
            }

            return null;
        }


        public override string GetDisplayTitle()
        {
            var settings = (RenderWithMaterial) target;
            var material = settings.material.value;
            return ObjectNames.GetInspectorTitle(material);
        }

        protected void MaterialPropertiesGui(Material material)
        {
            if (material == null)
                return;

            if (materialEditor == null || cachedMaterial != material)
            {
                cachedMaterial = material;
                UnityEditor.Editor.CreateCachedEditor(cachedMaterial, typeof(MaterialEditor), ref cachedEditor);
                materialEditor = (MaterialEditor) cachedEditor;
                properties = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] {cachedMaterial});
            }

            materialEditor.PropertiesDefaultGUI(properties);
        }


        protected static SerializedParameterOverride CreateSerializedParameterOverride(SerializedProperty property,
            Attribute[] attributes)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            return (SerializedParameterOverride) Activator.CreateInstance(typeof(SerializedParameterOverride), flags,
                null,
                new object[] {property, attributes}, null);
        }

        protected void InitInternals()
        {
            target = (PostProcessEffectSettings) typeof(PostProcessEffectBaseEditor)
                .GetProperty("target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(this);

            serializedObject = (SerializedObject) typeof(PostProcessEffectBaseEditor)
                .GetProperty("serializedObject", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(this);
        }

        protected void FindProperties()
        {
            m_Parameters = new List<SerializedParameterOverride>();

            var fields = target.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(t => t.FieldType.IsSubclassOf(typeof(ParameterOverride)) && t.Name != "enabled")
                .Where(t =>
                    (t.IsPublic && t.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length == 0)
                    || (t.GetCustomAttributes(typeof(UnityEngine.SerializeField), false).Length > 0)
                )
                .ToList();

            foreach (var field in fields)
            {
                var property = serializedObject.FindProperty(field.Name);
                var attributes = field.GetCustomAttributes(false).Cast<Attribute>().ToArray();
                var parameter = CreateSerializedParameterOverride(property, attributes);
                m_Parameters.Add(parameter);
            }
        }

        protected void OnInspectorGuiDefault()
        {
            foreach (var parameter in m_Parameters)
                PropertyField(parameter);
        }
    }
}