#ifndef UNLIT_PASS_HLSL
    #define UNLIT_PASS_HLSL

    #include "../ShaderLibrary/Common.hlsl"


    TEXTURE2D(_BaseMap);
    SAMPLER(sampler_BaseMap);

    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#endif