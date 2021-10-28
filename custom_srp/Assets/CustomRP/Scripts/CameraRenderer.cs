using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public partial class CameraRenderer
    {
        const string BufferName = "Render Camera";
        static readonly ShaderTagId UnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        static readonly ShaderTagId LitShaderTagId = new ShaderTagId("CustomLit");

        CommandBuffer m_cmdBuffer = new CommandBuffer()
        {
            name = BufferName
        };

        ScriptableRenderContext m_ctx;
        Camera m_camera;
        CullingResults m_cullingResults;

        Lighting m_lighting = new Lighting();

#if UNITY_EDITOR
        string SampleName { get; set; }
#else
    const string SampleName = BufferName;
#endif

        public void Render(ref ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
        {
            m_ctx = context;
            m_camera = camera;

            PrepareBuffer();
            PrepareForSceneWindow();

            if (!Cull(shadowSettings.MaxDistance))
                return;
            m_cmdBuffer.BeginSample(SampleName);
            ExecuteBuffer();
            m_lighting.Setup(ref context, ref m_cullingResults, shadowSettings);
            m_cmdBuffer.EndSample(SampleName);
            Setup();
            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
            DrawUnsupportedShaders();
            DrawGizmos();
            m_lighting.Cleanup(ref context);
            Submit();
        }

        partial void PrepareBuffer();
        partial void PrepareForSceneWindow();
        partial void DrawGizmos();
        partial void DrawUnsupportedShaders();



        bool Cull(float maxShadowDistance)
        {
            if (!m_camera.TryGetCullingParameters(out var p))
                return false;
            p.shadowDistance = Mathf.Min(m_camera.farClipPlane, maxShadowDistance);
            m_cullingResults = m_ctx.Cull(ref p);
            return true;
        }

        void Setup()
        {
            m_ctx.SetupCameraProperties(m_camera);

            var flags = m_camera.clearFlags;

            m_cmdBuffer.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? m_camera.backgroundColor.linear : Color.clear);

            m_cmdBuffer.BeginSample(SampleName);

            ExecuteBuffer();
        }

        void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
        {
            var sortingSettings = new SortingSettings(m_camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawSettings = new DrawingSettings(UnlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing,
            };
            drawSettings.SetShaderPassName(1, LitShaderTagId);
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);

            m_ctx.DrawRenderers(m_cullingResults, ref drawSettings, ref filterSettings);

            m_ctx.DrawSkybox(m_camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawSettings.sortingSettings = sortingSettings;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;

            m_ctx.DrawRenderers(m_cullingResults, ref drawSettings, ref filterSettings);
        }

        void Submit()
        {
            m_cmdBuffer.EndSample(SampleName);
            ExecuteBuffer();
            m_ctx.Submit();
        }

        void ExecuteBuffer()
        {
            m_ctx.ExecuteCommandBuffer(m_cmdBuffer);
            m_cmdBuffer.Clear();
        }
    }
}