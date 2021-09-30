Shader "Custom RP/Unlit"
{
    Properties
    {
        _BaseMap("Texture",2D) = "white" {}
        _BaseColor("Color",Color) = (1.0,1.0,1.0,1.0)
        _Cutoff ("Alpha Cutoff",Range(0.0,1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping",Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite("Z Write",Float) = 1
    }

    CustomEditor "CustomRP.CustomLitShaderGUI"

    SubShader
    {
        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"

            struct Attributes{
                float3 position:POSITION;
                float2 baseUV:TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS:SV_POSITION;
                float2 baseUV:VAR_BASE_UV;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UnlitPassVertex(Attributes input){
                Varyings ouput;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input,ouput);
                float3 world_position = TransformObjectToWorld(input.position.xyz);
                ouput.positionCS = TransformWorldToHClip(world_position); 

                float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
                ouput.baseUV = input.baseUV * baseST.xy + baseST.zw;
                return ouput;
            }

            float4 UnlitPassFragment(Varyings input):SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(input);

                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV);
                float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
                float4 final = baseMap * baseColor;
                #if defined(_CLIPPING)
                    clip(final.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
                #endif
                return final;
            }

            ENDHLSL
        }
    }
}
