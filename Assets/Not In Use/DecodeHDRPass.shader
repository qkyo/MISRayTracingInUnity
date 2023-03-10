Shader "RenderGraph/DecodeHDRPass"
{
	Properties
	{
	}
	SubShader
	{
		Pass
		{
			Name "DecodeHDRPass"

			Cull Off ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"

			sampler2D _Tex;
			half4 _Tex_HDR;
			half4 _Tint = (0.7, 0.7, 0.7, 1);
			half _Exposure = 1.0;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex.xyz);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half4 tex = tex2D(_Tex, i.uv);
				half3 c = DecodeHDR(tex, _Tex_HDR);
				c = c * half3(0.7, 0.7, 0.7) * unity_ColorSpaceDouble.rgb;
				c *= LinearToGammaSpace(_Exposure);
				return half4(c, 1);
			}
			ENDCG
		}
	}
}
