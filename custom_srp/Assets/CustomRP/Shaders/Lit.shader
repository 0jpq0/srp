Shader "Custom RP/Lit"
{
    Properties
    {
        _BaseMap("Texture",2D) = "white" {}
        _BaseColor("Color",Color) = (0.5,0.5,0.5,1.0)
        _Cutoff ("Alpha Cutoff",Range(0.0,1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping",Float) = 0
        [Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows ("Receive Shadows", Float) = 1
        [KeywordEnum(On, Clip, Dither, Off)] _Shadows ("Shadows", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite("Z Write",Float) = 1
        _Metallic("Metallic",Range(0,1)) = 0
        _Smoothness("Smoothness",Range(0,1)) = 0.5
        [Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha("Premultiply Alpha",Float) = 0
    }

    CustomEditor "CustomRP.CustomLitShaderGUI"

    SubShader
    {
        Pass
        {
            Tags {
                "LightMode" = "CustomLit"
            }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _RECEIVE_SHADOWS
            #pragma shader_feature _PREMULTIPLY_ALPHA
            #pragma shader_feature _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
            #pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER
            #pragma multi_compile_instancing
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "LitPass.hlsl"

            struct Attributes{
                float3 position:POSITION;
                float3 normal:NORMAL;
                float2 baseUV:TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS:SV_POSITION;
                float3 positionWS:VAR_POSITION;
                float3 normal:VAR_NORMAL;
                float2 baseUV:VAR_BASE_UV;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings LitPassVertex(Attributes input){
                Varyings ouput;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input,ouput);
                float3 world_position = TransformObjectToWorld(input.position.xyz);
                ouput.positionWS = world_position;
                ouput.positionCS = TransformWorldToHClip(world_position); 
                ouput.normal = TransformObjectToWorldNormal(input.normal);

                float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
                ouput.baseUV = input.baseUV * baseST.xy + baseST.zw;
                return ouput;
            }

            float4 LitPassFragment(Varyings input):SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(input);

                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV);
                float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
                float4 final = baseMap * baseColor;
                #if defined(_CLIPPING)
                    clip(final.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
                #endif

                // final.rgb = abs(length(input.normal)-1.0)*10.0;
                // final.rgb = normalize(input.normal);
                // final.rgb = input.normal;

                Surface surface;

                surface.position = input.positionWS;
                surface.normal = normalize(input.normal);
                surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
                surface.depth = -TransformWorldToView(input.positionWS).z;
                surface.color = final.rgb;
                surface.alpha = final.a;
                surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnitPerMaterial,_Metallic);
                surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnitPerMaterial,_Smoothness);
                surface.dither = InterleavedGradientNoise(input.positionCS.xy, 0);
                #if defined(_PREMULTIPLY_ALPHA)
                    BRDF brdf = GetBRDF(surface,true);
                #else
                    BRDF brdf = GetBRDF(surface);
                #endif

                float3 color = GetLighting(surface,brdf);

                return float4(color,surface.alpha);
            }

            ENDHLSL
        }

        Pass{
            Tags{"LightMode" = "ShadowCaster"}
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            // #pragma shader_feature _CLIPPING
            #pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
            #pragma multi_compile_instancing
            #pragma vertex ShadowCasterPassVertex
            #pragma fragment ShadowCasterPassFragment
            #include "ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
