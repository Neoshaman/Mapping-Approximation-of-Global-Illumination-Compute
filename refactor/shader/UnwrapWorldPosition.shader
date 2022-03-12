Shader "MAGIC/UnwrapWorldPosition"
{
    // Properties
    // {
    //     _MainTex ("Texture", 2D) = "white" {}
    // }
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
                float2 uv		: TEXCOORD0;
            };

            struct rasterData
            {
                float4 vertex	: POSITION;
                float4 position : COLOR;
            };

            rasterData vertexProgram ( meshData input )
            {
                rasterData output;
                    output.vertex = unwrap(input.uv, input.vertex.w);
                    output.position = input.vertex;
                return output;
            }

            fixed4 fragmentProcessing ( rasterData input ) : SV_Target
            {
                return input.position/64;
            }
            ENDCG
        }
    }
}
