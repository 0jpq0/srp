#ifndef SURFACE_HLSL
    #define SURFACE_HLSL

    struct Surface{
        float3 position;
        float3 normal;
        float3 viewDirection;
        float depth;
        float3 color;
        float alpha;
        float metallic;
        float smoothness;
        float dither;
    };
    
#endif