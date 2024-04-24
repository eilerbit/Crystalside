#ifndef CUSTOM_LIT_COMMON_INCLUDED
#define CUSTOM_LIT_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_ColorMap); SAMPLER(sampler_ColorMap); // RGB = albedo, A = alpha

CBUFFER_START(UnityPerMaterial)            
float4 _ColorMap_ST; // This is automatically set by Unity. Used in TRANSFORM_TEX to apply UV tiling
float4 _ColorTint;
float _Cutoff;
float _Smoothness;
CBUFFER_END

void PerformAlphaClip(float4 colorSample)
{
#ifdef _ALPHA_CUTOUT
    clip(colorSample.a * _ColorTint.a - _Cutoff);
#endif	
}

#endif
