#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.Profiling;

namespace CustomRP
{
    public partial class CameraRenderer
    {
        static readonly ShaderTagId[] LegacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
    };

        static Material s_errorMaterial;

        partial void DrawUnsupportedShaders()
        {
            if (s_errorMaterial == null)
                s_errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));

            var drawSettings = new DrawingSettings(LegacyShaderTagIds[0], new SortingSettings(m_camera))
            {
                overrideMaterial = s_errorMaterial
            };

            for (int i = 1; i < LegacyShaderTagIds.Length; i++)
                drawSettings.SetShaderPassName(i, LegacyShaderTagIds[i]);

            var filterSettings = FilteringSettings.defaultValue;

            m_ctx.DrawRenderers(m_cullingResults, ref drawSettings, ref filterSettings);
        }

        partial void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos())
            {
                m_ctx.DrawGizmos(m_camera, GizmoSubset.PreImageEffects);
                m_ctx.DrawGizmos(m_camera, GizmoSubset.PostImageEffects);
            }
        }

        partial void PrepareBuffer()
        {
            Profiler.BeginSample("Editor Only");
            m_cmdBuffer.name = SampleName = m_camera.name;
            Profiler.EndSample();
        }

        partial void PrepareForSceneWindow()
        {
            if (m_camera.cameraType == CameraType.SceneView)
                ScriptableRenderContext.EmitWorldGeometryForSceneView(m_camera);
        }
    }
}

#endif