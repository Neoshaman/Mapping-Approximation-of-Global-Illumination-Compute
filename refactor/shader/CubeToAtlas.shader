Shader "MAGIC/CubeToAtlas"
{
     Properties
    {
        _Cube ("Cubemap", CUBE) = "" {}
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
                float4 vertex : SV_POSITION;
            };

            samplerCUBE _Cube;

            rasterData vertexProgram ( meshData input )
            {
                rasterData output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            fixed4 fragmentProcessing ( rasterData input ) : SV_Target
            {
            	float2 g = input.uv - 0.5;
                float3 normal = UnpackNormalFromOct(2*g);

                return texCUBE(_Cube, normal);
            }
            ENDCG
        }
    }
}
