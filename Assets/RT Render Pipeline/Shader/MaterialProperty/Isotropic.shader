Shader "RayTracing/Isotropic"
{
  Properties
  {
    _Color("Main Color", Color) = (1,1,1,1)
    _Density("Density", Float) = 0.01
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
      };

      struct v2f
      {
        float3 normal : TEXCOORD0;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
      };

      CBUFFER_START(UnityPerMaterial)
      half4 _Color;
      CBUFFER_END

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        UNITY_TRANSFER_FOG(o, o.vertex);
        return o;
      }

      half4 frag(v2f i) : SV_Target
      {
        half d = max(dot(i.normal, float3(0.0f, 1.0f, 0.0f)), 0.5f);
        half4 col = half4((_Color * d).rgb, 1.0f);
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
      #include "../../ShaderLibrary/Common.hlsl"
      #include "../../ShaderLibrary/PRNG.hlsl"
      
      CBUFFER_START(UnityPerMaterial)
      float4 _Color;
      float _Density;
      CBUFFER_END

      [shader("closesthit")]
      void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
      {
        if (rayIntersection.remainingDepth < 0) // is inner ray. Enter Volumn, can not come out.
        { 
          // record hit point t parameter
          rayIntersection.hitT = RayTCurrent();
          return;
        }

        float t1 = RayTCurrent();

        // Get position in world space.
        RayDesc rayDescriptor;
        rayDescriptor.Origin = WorldRayOrigin();
        rayDescriptor.Direction = WorldRayDirection();
        rayDescriptor.TMin = t1 + 1e-5f;
        rayDescriptor.TMax = _CameraFarDistance;

        RayIntersection innerRayIntersection;
        innerRayIntersection.remainingDepth = min(-1, rayIntersection.remainingDepth - 1);
        innerRayIntersection.PRNGStates = rayIntersection.PRNGStates;
        innerRayIntersection.color = float4(0, 0, 0, 0);
        innerRayIntersection.hitT = 0.0f;
      }
      ENDHLSL
    }
  }
}