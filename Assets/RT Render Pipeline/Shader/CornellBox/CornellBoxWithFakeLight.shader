Shader "RayTracing/CornellBoxWithFakeLight"
{
  Properties
  {
    _Color("Main Color", Color) = (1,1,1,1)
    _BaseColorMap("BaseColorMap", 2D) = "white" {}
  }
  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    LOD 100

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      // make fog work
      #pragma multi_compile_fog

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float3 normal : TEXCOORD0;
        float2 uv : TEXCOORD1;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
      };

      sampler2D _BaseColorMap;
      CBUFFER_START(UnityPerMaterial)
      float4 _BaseColorMap_ST;
      half4 _Color;
      CBUFFER_END

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.uv = TRANSFORM_TEX(v.uv, _BaseColorMap);
        UNITY_TRANSFER_FOG(o, o.vertex);
        return o;
      }

      half4 frag(v2f i) : SV_Target
      {
        half d = max(dot(i.normal, float3(0.0f, 1.0f, 0.0f)), 0.5f);
        half4 col = half4((_Color * d).rgb, 1.0f);
        col *= tex2D(_BaseColorMap, i.uv);
        // apply fog
        UNITY_APPLY_FOG(i.fogCoord, col);
        return col;
      }
      ENDCG
    }
  }
  SubShader
  {
    Pass
    {
      Name "RayTracing"
      Tags { "LightMode" = "RayTracing" }

      HLSLPROGRAM

      #pragma raytracing test
      // #pragma multi_compile _ _ACCUMULATE_AVERAGE_SAMPLE_ON
      #include "../../ShaderLibrary/Common.hlsl"
      #include "../../ShaderLibrary/PRNG.hlsl"
      #include "../../ShaderLibrary/ONB.hlsl"
      
      struct IntersectionVertex
      {
          // Object space normal of the vertex
          float3 normalOS;
          float2 texCoord0;
      };

      // Send data to GPU buffer
      TEXTURE2D(_BaseColorMap);
      SAMPLER(sampler_BaseColorMap);
      CBUFFER_START(UnityPerMaterial)
      float4 _BaseColorMap_ST;
      float4 _Color;
      CBUFFER_END

      void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex)
      {
        // Get vertex normal data in object space.
        outVertex.normalOS = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);
        // Get the UV
        outVertex.texCoord0 = UnityRayTracingFetchVertexAttribute2(vertexIndex, kVertexAttributeTexCoord0);
      }

      float ScatteringPDF(float3 inOrigin, float3 inDirection, float inT, float3 hitNormal, float3 scatteredDir)
      {
        float cosine = dot(hitNormal, scatteredDir);
        return max(0.0f, cosine / M_PI);
      }

      [shader("closesthit")]
      void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload,
                            AttributeData attributeData : SV_IntersectionAttributes)
      {
        // Fetch the indices of the currentr triangle
        // Get the index value of the ray traced hit triangle.
        uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

        // Fetch the 3 vertices
        IntersectionVertex v0, v1, v2;
        FetchIntersectionVertex(triangleIndices.x, v0);
        FetchIntersectionVertex(triangleIndices.y, v1);
        FetchIntersectionVertex(triangleIndices.z, v2);

        // Compute the full barycentric coordinates
        float3 barycentricCoordinates = float3(1.0 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x, attributeData.barycentrics.y);

        // Interpolation to calculate the specific information of the collision point
        float3 normalOS = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS, barycentricCoordinates);
        float2 texCoord0 = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.texCoord0, v1.texCoord0, v2.texCoord0, barycentricCoordinates);
        // DXR object to world transform matrix
        float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
        float3 normalWS = normalize(mul(objectToWorld, normalOS));
        float4 texColor = _Color * _BaseColorMap.SampleLevel(sampler_BaseColorMap, texCoord0, 0);

        float4 color = float4(0, 0, 0, 1);
        if (rayIntersection.remainingDepth > 0)   // still remain recursive step or recursive depth
        {
          // Get current hit position in world space.
          float3 origin = WorldRayOrigin();
          float3 direction = WorldRayDirection();
          float t = RayTCurrent();
          float3 positionWS = origin + direction * t;

          // Prepare to make reflection ray.
          // Build a orthonormal base according to normal vector in surface
          ONB uvw;
          ONBBuildFromW(uvw, normalWS);
          float3 scatteredDir;
          float pdf;

          // "sample the based-on-normal-vector ONB sphere"
          if (GetRandomValue(rayIntersection.PRNGStates) < 0.5f)
          {
            scatteredDir = ONBLocal(uvw, GetRandomCosineDirection(rayIntersection.PRNGStates));
            pdf = dot(normalWS, scatteredDir) / M_PI;
          }
          // "Sample the light we define"
          else 
          {
            // 554.0f is the position.y in world space of light
            const float3 _FakeLightMin = float3(-106.5f, 554.0f, -52.5f);
            const float3 _FakeLightMax = float3(106.5f, 554.0f, 52.5f);

            // Random vector on the light area
            float3 onLight = float3(
              _FakeLightMin.x + GetRandomValue(rayIntersection.PRNGStates) * (_FakeLightMax.x - _FakeLightMin.x),
              _FakeLightMin.y,
              _FakeLightMin.z + GetRandomValue(rayIntersection.PRNGStates) * (_FakeLightMax.z - _FakeLightMin.z));

            float3 toLight = onLight - positionWS;
            float distanceSquared = toLight.x * toLight.x + toLight.y * toLight.y + toLight.z * toLight.z;
            toLight = normalize(toLight);

            // visible to light (surface is lighted)
            if (dot(toLight, normalWS) < 0.0f)
            {
              scatteredDir = ONBLocal(uvw, GetRandomCosineDirection(rayIntersection.PRNGStates));
              pdf = dot(normalWS, scatteredDir) / M_PI;
            }
            float lightArea = (_FakeLightMax.x - _FakeLightMin.x) * (_FakeLightMax.z - _FakeLightMin.z);
            float lightConsin = abs(toLight.y);

            if (lightConsin < 1e-5f)
            {
              scatteredDir = ONBLocal(uvw, GetRandomCosineDirection(rayIntersection.PRNGStates));
              pdf = dot(normalWS, scatteredDir) / M_PI;
            }
            scatteredDir = toLight;
            pdf = distanceSquared / (lightConsin * lightArea);

          }

          // Make reflection ray.
          RayDesc rayDescriptor;
          rayDescriptor.Origin = positionWS + 0.001f * normalWS;
          rayDescriptor.Direction = scatteredDir;
          rayDescriptor.TMin = 1e-5f;
          rayDescriptor.TMax = _CameraFarDistance;

          // Tracing reflection.
          RayIntersection reflectionRayIntersection;
          reflectionRayIntersection.remainingDepth = rayIntersection.remainingDepth - 1;
          reflectionRayIntersection.PRNGStates = rayIntersection.PRNGStates;
          reflectionRayIntersection.color = float4(0.0f, 0.0f, 0.0f, 0.0f);


          TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, reflectionRayIntersection);
          
          rayIntersection.PRNGStates = reflectionRayIntersection.PRNGStates;
          color = ScatteringPDF(origin, direction, t, normalWS, scatteredDir) * reflectionRayIntersection.color / pdf;
          color = max(float4(0, 0, 0, 0), color);
        }

        rayIntersection.color = texColor * 0.5f * color;
      }
      ENDHLSL
    }
  }
}