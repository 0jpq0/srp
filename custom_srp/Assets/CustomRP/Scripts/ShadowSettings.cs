using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomRP
{
    [System.Serializable]
    public class ShadowSettings
    {
        public enum TextureSize
        {
            _256 = 256, _512 = 512, _1024 = 1024,
            _2048 = 2048, _4096 = 4096, _8192 = 8192
        }

        public enum FilterMode
        {
            PCF2x2, PCF3x3, PCF5x5, PCF7x7
        }

        [System.Serializable]
        public struct Directional
        {
            public enum CascadeBlendMode
            {
                Hard, Soft, Dither
            }

            public CascadeBlendMode CascadeBlend;
            public TextureSize AtlasSize;

            public FilterMode Filter;

            [Range(1, 4)]
            public int CascadeCount;

            [Range(0f, 1f)]
            public float CascadeRatio1, CascadeRatio2, CascadeRatio3;

            [Range(0.001f, 1f)]
            public float CascadeFade;

            public Vector3 CascadeRatios => new Vector3(CascadeRatio1, CascadeRatio2, CascadeRatio3);
        }

        [Min(0.001f)]
        public float MaxDistance = 100f;

        [Range(0.001f, 1f)]
        public float DistanceFade = 0.1f;

        public Directional directional = new Directional
        {
            AtlasSize = TextureSize._1024,
            Filter = FilterMode.PCF2x2,
            CascadeCount = 4,
            CascadeRatio1 = 0.1f,
            CascadeRatio2 = 0.25f,
            CascadeRatio3 = 0.5f,
            CascadeFade = 0.1f,
            CascadeBlend = Directional.CascadeBlendMode.Hard
        };
    }
}
