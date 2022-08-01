Shader "MAGIC/WorldPosition"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float3 position : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
				v2f o;
				o.vertex = UNWRAP(v.vertex, v.uv);
				o.position = CONVERTpositionTOtexture(v.vertex);//TODO: normalize and scale for 8Bit texture data
				return o;
            }

            fixed4 frag (v2f fragmentInput) : SV_Target
            {
				return float4(fragmentInput.position, 1);
			}
            ENDCG
        }
    }
}
