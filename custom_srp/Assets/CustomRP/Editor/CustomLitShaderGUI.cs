using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace CustomRP
{
    public class CustomLitShaderGUI : ShaderGUI
    {
        MaterialEditor m_editor;
        Object[] m_materials;
        MaterialProperty[] m_properties;

        bool m_showPresets;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            m_editor = materialEditor;
            m_materials = materialEditor.targets;
            m_properties = properties;

            EditorGUILayout.Space();
            m_showPresets = EditorGUILayout.Foldout(m_showPresets, "Presets", true);
            if (m_showPresets)
            {
                OpaquePreset();
                ClipPreset();
                FadePreset();
                TransparentPreset();
            }
        }

        bool PresetButton(string name)
        {
            var btn = GUILayout.Button(name);

            if (btn)
                m_editor.RegisterPropertyChangeUndo(name);

            return btn;
        }

        void OpaquePreset()
        {
            if (PresetButton("Opaque"))
            {
                Clipping = false;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.Zero;
                ZWrite = true;
                RenderQueue = RenderQueue.Geometry;
            }
        }

        void ClipPreset()
        {
            if (PresetButton("Clip"))
            {
                Clipping = true;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.Zero;
                ZWrite = true;
                RenderQueue = RenderQueue.AlphaTest;
            }
        }

        void FadePreset()
        {
            if (PresetButton("Fade"))
            {
                Clipping = false;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.SrcAlpha;
                DstBlend = BlendMode.OneMinusSrcAlpha;
                ZWrite = false;
                RenderQueue = RenderQueue.Transparent;
            }
        }

        void TransparentPreset()
        {
            if (HasPremultiplyAlpha && PresetButton("Transparent"))
            {
                Clipping = false;
                PremultiplyAlpha = true;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.OneMinusSrcAlpha;
                ZWrite = false;
                RenderQueue = RenderQueue.Transparent;
            }
        }

        bool SetProperty(string name, float value)
        {
            var p = FindProperty(name, m_properties);
            if (p != null)
            {
                p.floatValue = value;
                return true;
            }

            return false;
        }

        void SetKeyword(string keyword, bool enable)
        {
            if (enable)
            {
                foreach (Material m in m_materials)
                    m.EnableKeyword(keyword);
            }
            else
            {
                foreach (Material m in m_materials)
                    m.DisableKeyword(keyword);
            }
        }

        void SetProperty(string name, string keyword, bool value)
        {
            if (SetProperty(name, value ? 1f : 0f))
                SetKeyword(keyword, value);
        }

        bool HasProperty(string name) =>
            FindProperty(name, m_properties, false) != null;

        bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");

        bool Clipping
        {
            set => SetProperty("_Clipping", "_CLIPPING", value);
        }

        bool PremultiplyAlpha
        {
            set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
        }

        BlendMode SrcBlend
        {
            set => SetProperty("_SrcBlend", (float)value);
        }

        BlendMode DstBlend
        {
            set => SetProperty("_DstBlend", (float)value);
        }

        bool ZWrite
        {
            set => SetProperty("_ZWrite", value ? 1f : 0f);
        }

        RenderQueue RenderQueue
        {
            set
            {
                foreach (Material m in m_materials)
                    m.renderQueue = (int)value;
            }
        }
    }
}
