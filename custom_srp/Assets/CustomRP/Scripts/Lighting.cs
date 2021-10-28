using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public class Lighting
    {
        const string BufferName = "Lighting";

        const int MaxDirLightCount = 4;
        static readonly int DirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
        static readonly int DirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
        static readonly int DirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
        static readonly int DirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

        static Vector4[] s_dirLightColors = new Vector4[MaxDirLightCount];
        static Vector4[] s_dirLightDirections = new Vector4[MaxDirLightCount];
        static Vector4[] s_dirLightShadowData = new Vector4[MaxDirLightCount];

        CommandBuffer m_cmdBuffer = new CommandBuffer() { name = BufferName };

        Shadows m_shadows = new Shadows();

        public void Setup(ref ScriptableRenderContext context, ref CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            m_cmdBuffer.BeginSample(BufferName);
            m_shadows.Setup(ref context, ref cullingResults, shadowSettings);
            SetupLights(ref cullingResults);
            m_shadows.Render(ref context, ref cullingResults, shadowSettings);
            m_cmdBuffer.EndSample(BufferName);
            context.ExecuteCommandBuffer(m_cmdBuffer);
            m_cmdBuffer.Clear();

        }

        void SetupLights(ref CullingResults cullingResults)
        {
            var visibleLights = cullingResults.visibleLights;
            var dirLightCount = 0;
            for (int i = 0; i < visibleLights.Length; ++i)
            {
                var visibleLight = visibleLights[i];
                if (visibleLight.lightType == LightType.Directional)
                {
                    SetupDirectionalLight(dirLightCount++, ref visibleLight, ref cullingResults);
                    if (dirLightCount >= MaxDirLightCount)
                        break;
                }
            }

            m_cmdBuffer.SetGlobalInt(DirLightCountId, dirLightCount);
            m_cmdBuffer.SetGlobalVectorArray(DirLightColorsId, s_dirLightColors);
            m_cmdBuffer.SetGlobalVectorArray(DirLightDirectionsId, s_dirLightDirections);
            m_cmdBuffer.SetGlobalVectorArray(DirLightShadowDataId, s_dirLightShadowData);
        }

        void SetupDirectionalLight(int index, ref VisibleLight visibleLight, ref CullingResults cullingResults)
        {
            s_dirLightColors[index] = visibleLight.finalColor;
            s_dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
            s_dirLightShadowData[index] = m_shadows.ReserveDirectionalShadows(visibleLight.light, index, ref cullingResults);
        }

        public void Cleanup(ref ScriptableRenderContext ctx)
        {
            m_shadows.Cleanup(ref ctx);
        }
    }
}