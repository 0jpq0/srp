using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    [CreateAssetMenu(menuName = "Rendering/Custom srp")]
    public class CustomRenderPiplineAsset : RenderPipelineAsset
    {
        [SerializeField]
        bool m_useDynamicBatching;

        [SerializeField]
        bool m_useGPUInstancing;

        [SerializeField]
        bool m_useSRPBatcher;

        [SerializeField]
        ShadowSettings m_shadows = default;

        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipline(m_useDynamicBatching, m_useGPUInstancing, m_useSRPBatcher, m_shadows);
        }
    }
}