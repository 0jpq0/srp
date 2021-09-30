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

        static Vector4[] s_dirLightColors = new Vector4[MaxDirLightCount];
        static Vector4[] s_dirLightDirections = new Vector4[MaxDirLightCount];

        CommandBuffer m_cmdBuffer = new CommandBuffer() { name = BufferName };


        public void Setup(ref ScriptableRenderContext context, ref CullingResults cullingResults)
        {

            m_cmdBuffer.BeginSample(BufferName);
            SetupLights(ref cullingResults);
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
                    SetupDirectionalLight(dirLightCount++, ref visibleLight);
                    if (dirLightCount >= MaxDirLightCount)
                        break;
                }
            }

            m_cmdBuffer.SetGlobalInt(DirLightCountId, dirLightCount);
            m_cmdBuffer.SetGlobalVectorArray(DirLightColorsId, s_dirLightColors);
            m_cmdBuffer.SetGlobalVectorArray(DirLightDirectionsId, s_dirLightDirections);
        }

        void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
        {
            s_dirLightColors[index] = visibleLight.finalColor;
            s_dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        }

    }
}