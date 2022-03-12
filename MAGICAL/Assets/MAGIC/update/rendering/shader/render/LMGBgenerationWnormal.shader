Shader "MAGIC/LMGBgenerationWnormal"
{
    // Properties
    // {
    //     _MainTex ("Texture", 2D) = "white" {}
    // }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "../MAGIC.cginc"

            struct d
            {
                float4 vertex	: POSITION;
                float2 uv		: TEXCOORD1;
                float3 normal   : NORMAL;
            };

            struct v2f
            {
                float4 vertex	: POSITION;
                float3 wnormal  : TEXCOORD0;
            };
			//------------------------------------
            v2f vert (d v)
            {
                v2f o;
                float2 uv = float2(v.uv.x, 1-v.uv.y);
                o.vertex = float4((uv.xy*2)-1,1,v.vertex.w);
                o.wnormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                return half4(i.wnormal*0.5+0.5,1);
            }
            ENDCG
        }
    }
}
