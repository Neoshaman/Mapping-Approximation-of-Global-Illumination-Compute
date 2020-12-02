Shader "MAGIC/WorldNormal"
{
	//TODO: make a version that takes tangent normal map
	//Properties
	//{
	//	_Normal("Texture", 2D) = "bump" {}
	//}
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
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float3 normal : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UNWRAP(v.vertex, v.uv);
				o.normal = CONVERTnormalTOtexture(v.normal);//TODO: normalize and scale for 8Bit texture data
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(i.normal, 1);
			}
			ENDCG
		}
	}
}
