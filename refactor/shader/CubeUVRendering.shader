Shader "MAGIC/CubeUVRendering"
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
            #pragma vertex vertexProgram
            #pragma fragment fragmentProcessing

            #include "UnityCG.cginc"
            #include "ShaderTools.cginc"

            struct meshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct rasterData
            {
                float2 uv : TEXCOORD0;
                // float2 debug : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            rasterData vertexProgram ( meshData input )
            {
                rasterData output;
                    output.vertex = UnityObjectToClipPos(input.vertex);
                    output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                    // //debug
                    // float4 wpos = mul(unity_ObjectToWorld, input.vertex);
                    // output.debug = floor(wpos.xz/4)/16;
                return output;
            }

            fixed4 fragmentProcessing ( rasterData input ) : SV_Target
            {
            	//add blue when no uv ->skybox background color,//initialized within code
            	//depth
                return float4 (input.uv,0,0);
            }
            ENDCG
        }
    }
}
