#ifndef CUSTOM_RT_COMMON_INCLUDED
#define CUSTOM_RT_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "UnityRaytracingMeshUtils.cginc"

#define CBUFFER_START(name) cbuffer name {
#define CBUFFER_END };

// Macro that interpolate any attribute using barycentric coordinates
#define INTERPOLATE_RAYTRACING_ATTRIBUTE(A0, A1, A2, BARYCENTRIC_COORDINATES) (A0 * BARYCENTRIC_COORDINATES.x + A1 * BARYCENTRIC_COORDINATES.y + A2 * BARYCENTRIC_COORDINATES.z)

#define TEXTURE2D(textureName) Texture2D textureName
#define SAMPLER(samplerName) SamplerState samplerName

CBUFFER_START(CameraBuffer)
float4x4 _InvCameraViewProj;
float3 _WorldSpaceCameraPos;
float _CameraFarDistance;
CBUFFER_END

#include "Ray.hlsl"

inline void GenerateCameraRay(out float3 origin, out float3 direction)
{
    // center in the middle of the pixel.
    float2 xy = DispatchRaysIndex().xy + 0.5f;

    // the position of screen coordinate in Project Space
    float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;

    // the position of screen coordinate in World Space
    float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));

    world.xyz /= world.w;
    origin = _WorldSpaceCameraPos.xyz;
    direction = normalize(world.xyz - origin);
}

inline void GenerateCameraRayWithOffset(out float3 origin, out float3 direction, float2 offset)
{
    float2 xy = DispatchRaysIndex().xy + offset;
    float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;

    // Un project the pixel coordinate into a ray.
    float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));

    world.xyz /= world.w;
    origin = _WorldSpaceCameraPos.xyz;
    direction = normalize(world.xyz - origin);
}

float3 remapTo0_1(float3 vec)
{
    return 0.5f * (normalize(vec) + 1.0f);
}
#endif