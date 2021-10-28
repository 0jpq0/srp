#ifndef LIGHTING_HLSL
    #define LIGHTING_HLSL

    #include "surface.hlsl"
    #include "BRDF.hlsl"
    #include "Light.hlsl"

    float3 IncomingLight(Surface surface,Light light)
    {
        return saturate(dot(surface.normal,light.direction)*light.attenuation) * light.color;
        // return saturate(dot(surface.normal,light.direction)) * light.color;
    }
    
    float3 GetLighting(Surface surface,BRDF brdf,Light light){
        return IncomingLight(surface,light) * DirectBRDF(surface, brdf, light);
        // return light.attenuation;
    }
    
    float3 GetLighting(Surface surface,BRDF brdf){
        ShadowData shadowData = GetShadowData(surface);
        float3 color = 0.0;
        for(int i = 0;i < GetDirectionalLightCount();++i){
            Light light = GetDirectionalLight(i,surface,shadowData);
            color += GetLighting(surface,brdf,light);
        }

        return color;
    }

#endif