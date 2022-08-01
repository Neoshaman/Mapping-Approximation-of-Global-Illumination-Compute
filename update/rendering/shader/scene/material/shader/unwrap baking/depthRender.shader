Shader "MAGIC/DepthRender"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "../../../MAGIC.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float3 depth : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UNWRAP(v.vertex, v.uv);
				o.depth = CONVERTpositionTOtexture(v.vertex);//TODO: normalize and scale for 8Bit texture data, depth = abs(position - camera)
				return o;
			}

			fixed4 frag(v2f fragmentInput) : SV_Target
			{
				return float4(fragmentInput.depth, 1);
			}
			ENDCG
		}
	}
}
