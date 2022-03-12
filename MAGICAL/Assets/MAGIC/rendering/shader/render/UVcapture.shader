Shader "MAGIC/UVcapture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            // #include "UnityCustomRenderTexture.cginc"
            // #pragma vertex CustomRenderTextureVertexShader
            // #pragma fragment frag

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 debug : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                //debug
                float4 wpos = mul(unity_ObjectToWorld, v.vertex);
                o.debug = floor(wpos.xz/4)/16;
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
            	//add blue when no uv ->skybox background color,
            	//depth
                return float4 (i.uv,0,0);
            }
            ENDCG
        }
    }
}
