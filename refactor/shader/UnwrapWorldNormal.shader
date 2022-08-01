Shader "MAGIC/UnwrapWorldNormal"
{
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vertexProgram
            #pragma fragment fragmentProcessing
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "ShaderTools.cginc"

            struct meshData
            {
                float4 vertex	: POSITION;
                float2 uv		: TEXCOORD1;
                float3 normal   : NORMAL;
            };

            struct rasterData
            {
                float4 vertex	: POSITION;
                float3 wnormal  : TEXCOORD0;
            };

            rasterData vertexProgram ( meshData input )
            {
                rasterData output;
                    output.vertex = unwrap(input.uv, input.vertex.w);
                    output.wnormal = UnityObjectToWorldNormal(input.normal);
                return output;
            }

            fixed4 fragmentProcessing ( rasterData input ) : SV_Target
            {
                return half4(input.wnormal*0.5+0.5,1);
            }
            ENDCG
        }
    }
}
