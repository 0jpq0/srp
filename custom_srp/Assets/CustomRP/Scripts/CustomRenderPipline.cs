using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Diagnostics;

namespace CustomRP
{
    public class CustomRenderPipline : RenderPipeline
    {
        CommandBuffer m_cmdBuffer = new CommandBuffer { name = "Render Camera" };
        ShaderTagId m_shaerTagId = new ShaderTagId("SRPDefaultUnlit");
        Material m_errorMaterial;

        CameraRenderer m_renderer = new CameraRenderer();

        bool m_useDynamicBatching;
        bool m_useGPUInstancing;

        public CustomRenderPipline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
        {
            m_useDynamicBatching = useDynamicBatching;
            m_useGPUInstancing = useGPUInstancing;

            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
                m_renderer.Render(ref context, camera, m_useDynamicBatching, m_useGPUInstancing);
        }
    }
}