using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public class Shadows
    {
        const string BufferName = "Shadows";
        const int MaxShadowedDirectionalLightCount = 4;
        const int MaxCascades = 4;
        static readonly int DirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
        static readonly int DirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
        static readonly int CascadeCountId = Shader.PropertyToID("_CascadeCount");
        static readonly int CascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
        static readonly int CascadeDataId = Shader.PropertyToID("_CascadeData");
        static readonly int ShadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");
        static readonly int CascadeDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");

        static readonly string[] DirectionalFilterKeywords = {
            "_DIRECTIONAL_PCF3",
            "_DIRECTIONAL_PCF5",
            "_DIRECTIONAL_PCF7",
        };

        static readonly string[] CascadeBlendKeywords = {
            "_CASCADE_BLEND_SOFT",
            "_CASCADE_BLEND_DITHER"
        };

        static Matrix4x4[] s_dirShadowMaterices = new Matrix4x4[MaxShadowedDirectionalLightCount * MaxCascades];
        static Vector4[] s_cascadeCullingSpheres = new Vector4[MaxCascades];
        static Vector4[] s_cascadeData = new Vector4[MaxCascades];

        CommandBuffer m_buffer = new CommandBuffer
        {
            name = BufferName
        };

        struct ShadowedDirectionalLight
        {
            public int VisibleLightIndex;
            public float SlopeScaleBias;
            public float NearPlaneOffset;
        }

        ShadowedDirectionalLight[] m_shadowedDirectionalLights = new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];

        int m_shadowedDirectionalLightCount;
        ShadowSettings m_shadowSettins;

        public void Setup(ref ScriptableRenderContext ctx, ref CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            m_shadowSettins = shadowSettings;
            m_shadowedDirectionalLightCount = 0;
        }

        void ExecuteBuffer(ref ScriptableRenderContext ctx)
        {
            ctx.ExecuteCommandBuffer(m_buffer);
            m_buffer.Clear();
        }

        public Vector3 ReserveDirectionalShadows(Light light, int visibleLightIndex, ref CullingResults cullingResults)
        {
            if (m_shadowedDirectionalLightCount < MaxShadowedDirectionalLightCount &&
                light.shadows != LightShadows.None && light.shadowStrength > 0f &&
                cullingResults.GetShadowCasterBounds(visibleLightIndex, out var bounds))
            {
                m_shadowedDirectionalLights[m_shadowedDirectionalLightCount] = new ShadowedDirectionalLight
                {
                    VisibleLightIndex = visibleLightIndex,
                    SlopeScaleBias = light.shadowBias,
                    NearPlaneOffset = light.shadowNearPlane
                };

                return new Vector3(light.shadowStrength, m_shadowSettins.directional.CascadeCount * m_shadowedDirectionalLightCount++, light.shadowNormalBias);
            }

            return Vector3.zero;
        }

        public void Render(ref ScriptableRenderContext ctx, ref CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            if (m_shadowedDirectionalLightCount > 0)
                RenderDirectionalShadows(ref ctx, ref cullingResults, shadowSettings);
            else
                m_buffer.GetTemporaryRT(DirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }

        void RenderDirectionalShadows(ref ScriptableRenderContext ctx, ref CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            var atlasSize = (int)shadowSettings.directional.AtlasSize;
            m_buffer.GetTemporaryRT(DirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            m_buffer.SetRenderTarget(DirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            m_buffer.ClearRenderTarget(true, false, Color.clear);
            m_buffer.BeginSample(BufferName);
            ExecuteBuffer(ref ctx);

            int tiles = MaxShadowedDirectionalLightCount * m_shadowSettins.directional.CascadeCount;
            int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
            int tileSize = atlasSize / split;

            for (int i = 0; i < m_shadowedDirectionalLightCount; i++)
                RenderDirectionalShadows(ref ctx, ref cullingResults, i, split, tileSize, shadowSettings);

            m_buffer.SetGlobalInt(CascadeCountId, m_shadowSettins.directional.CascadeCount);
            m_buffer.SetGlobalVectorArray(CascadeCullingSpheresId, s_cascadeCullingSpheres);
            m_buffer.SetGlobalVectorArray(CascadeDataId, s_cascadeData);
            m_buffer.SetGlobalMatrixArray(DirShadowMatricesId, s_dirShadowMaterices);
            var f = 1 - m_shadowSettins.directional.CascadeFade;
            m_buffer.SetGlobalVector(CascadeDistanceFadeId, new Vector4(1f / m_shadowSettins.MaxDistance, 1 / m_shadowSettins.DistanceFade, 1 / (1 - f * f)));

            SetKeywords(DirectionalFilterKeywords, (int)shadowSettings.directional.Filter - 1);
            SetKeywords(CascadeBlendKeywords, (int)shadowSettings.directional.CascadeBlend - 1);
            m_buffer.SetGlobalVector(ShadowAtlasSizeId, new Vector4(atlasSize, 1f / atlasSize));

            m_buffer.EndSample(BufferName);
            ExecuteBuffer(ref ctx);
        }

        void SetKeywords(string[] keywords, int enabledIndex)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                if (i == enabledIndex)
                    m_buffer.EnableShaderKeyword(keywords[i]);
                else
                    m_buffer.DisableShaderKeyword(keywords[i]);
            }
        }

        void RenderDirectionalShadows(ref ScriptableRenderContext ctx, ref CullingResults cullingResults, int index, int split, int tileSize, ShadowSettings settings)
        {
            var light = m_shadowedDirectionalLights[index];

            var shadowSettings = new ShadowDrawingSettings(cullingResults, light.VisibleLightIndex);

            var cascadeCount = m_shadowSettins.directional.CascadeCount;
            var tileOffset = index * cascadeCount;
            Vector3 ratios = m_shadowSettins.directional.CascadeRatios;

            float cullingFactor =
                Mathf.Max(0f, 0.8f - settings.directional.CascadeFade);

            for (int i = 0; i < cascadeCount; i++)
            {
                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                    light.VisibleLightIndex, i, cascadeCount, ratios, tileSize, light.NearPlaneOffset,
                    out var viewMatrix, out var projectionMatrix, out var splitData);

                splitData.shadowCascadeBlendCullingFactor = cullingFactor;
                shadowSettings.splitData = splitData;

                if (index == 0)
                    SetCascadeData(i, splitData.cullingSphere, tileSize);

                var tileIndex = tileOffset + i;

                var offset = SetTileViewport(tileIndex, split, tileSize);

                s_dirShadowMaterices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, offset, split);

                m_buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

                m_buffer.SetGlobalDepthBias(0f, light.SlopeScaleBias);

                ExecuteBuffer(ref ctx);

                ctx.DrawShadows(ref shadowSettings);

                m_buffer.SetGlobalDepthBias(0, 0);
            }
        }

        void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
        {
            float texelSize = 2f * cullingSphere.w / tileSize;
            float filterSize = texelSize * ((float)m_shadowSettins.directional.Filter + 1f);
            cullingSphere.w -= filterSize;
            cullingSphere.w *= cullingSphere.w;
            s_cascadeCullingSpheres[index] = cullingSphere;
            s_cascadeData[index] = new Vector4(1f / cullingSphere.w, filterSize * 1.4142136f);

        }

        Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
        {
            if (SystemInfo.usesReversedZBuffer)
            {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }

            float scale = 1f / split;
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);

            // var scaleOffset = Matrix4x4.TRS(Vector3.one * 0.5f, Quaternion.identity, Vector3.one * 0.5f);
            // return scaleOffset * m;

            return m;
        }

        Vector2 SetTileViewport(int index, int split, float tileSize)
        {
            Vector2 offset = new Vector2(index % split, index / split);

            m_buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));

            return offset;
        }

        public void Cleanup(ref ScriptableRenderContext ctx)
        {
            m_buffer.ReleaseTemporaryRT(DirShadowAtlasId);
            ExecuteBuffer(ref ctx);
        }
    }
}