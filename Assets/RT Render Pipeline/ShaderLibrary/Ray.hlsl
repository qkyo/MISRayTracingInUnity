#ifndef CUSTOM_RAY_INCLUDED
#define CUSTOM_RAY_INCLUDED

RaytracingAccelerationStructure _AccelerationStructure;

struct RayIntersection
{
    int remainingDepth;
    uint4 PRNGStates;
    float4 color;
    float hitT;
    float3 normalWS;    // hit point normal
    float reflector;    // miss = 0, hit = 1;
    float3 direction;

    float kSpecular;
    float kDiffuse;
    float shininess;
};

struct AttributeData
{
    float2 barycentrics;
};

#endif