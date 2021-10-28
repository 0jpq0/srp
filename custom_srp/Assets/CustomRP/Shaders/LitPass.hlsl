#ifndef LIT_PASS_HLSL
    #define LIT_PASS_HLSL

    #include "../ShaderLibrary/Common.hlsl"
    #include "../ShaderLibrary/Surface.hlsl"
    #include "../ShaderLibrary/Shadows.hlsl"
    #include "../ShaderLibrary/Light.hlsl"
    #include "../ShaderLibrary/Lighting.hlsl"


    TEXTURE2D(_BaseMap);
    SAMPLER(sampler_BaseMap);

    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
    UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#endif