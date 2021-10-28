#ifndef COMMON_HLSL
    #define COMMON_HLSL

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl" 
    #include "UnityInput.hlsl"

    #define UNITY_MATRIX_M unity_ObjectToWorld
    #define UNITY_MATRIX_I_M unity_WorldToObject
    #define UNITY_MATRIX_V unity_MatrixV
    #define UNITY_MATRIX_VP unity_MatrixVP
    #define UNITY_MATRIX_P glstate_matrix_projection

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl" 
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl" 

    float Square(float v){
        return v*v;
    }

    float DistanceSquared(float3 pA,float3 pB){
        return dot(pA-pB,pA-pB);
    }
#endif