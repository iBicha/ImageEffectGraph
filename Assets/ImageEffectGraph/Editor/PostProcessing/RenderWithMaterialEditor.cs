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
        private Material cachedMaterial;

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
            //Show material properties
            var settings = (RenderWithMaterial) target;

            var guiEnabled = GUI.enabled;
            GUI.enabled = settings.material.overrideState;
            MaterialPropertiesGui(settings.material.value);
            GUI.enabled = guiEnabled;
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
                materialEditor =
                    (MaterialEditor) UnityEditor.Editor.CreateEditor(cachedMaterial, typeof(MaterialEditor));
            }

            materialEditor.PropertiesGUI();
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