Shader "MAGIC/UnwrapAlbedo"
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
                float4 vertex	: POSITION;
                float2 uv		: TEXCOORD1;
                fixed4 color    : COLOR;
            };

            struct rasterData
            {
                float4 vertex	: POSITION;
                fixed4 color    : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            rasterData vertexProgram ( meshData input )
            {
                rasterData output;
                output.vertex = unwrap(input.uv, input.vertex.w);
                output.color = input.color;
                return output;
            }

            fixed4 fragmentProcessing ( rasterData input ) : SV_Target
            {
                return fixed4(1,0,0,1) * input.color;;
            }
            ENDCG
        }
    }
}
