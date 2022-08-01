Shader "MAGIC/UnwrapShadowMask"
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

            #include "AutoLight.cginc"
            #include "UnityCG.cginc"
            #include "ShaderTools.cginc"

            struct meshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct rasterData
            {
                float4 vertex    : POSITION;
                float2 uv       : TEXCOORD1;
                LIGHTING_COORDS(2,3)
            };

            rasterData vertexProgram ( meshData input )
            {
                rasterData output;
                output.vertex = unwrap(input.uv, input.vertex.w);
                output.uv = input.uv;
                return output;
            }

            fixed4 fragmentProcessing ( rasterData input ) : SV_Target
            {
                float atten = LIGHT_ATTENUATION(i); // This is a float for your shadow/attenuation value, multiply your lighting value by this to get shadows. Replace i with whatever you've defined your input struct to be called (e.g. frag(v2f [b]i[/b]) : COLOR { ... ).
                float4 f1 = UNITY_SAMPLE_TEX2D(unity_Lightmap, input.uv[1]);
                float3 f2 = DecodeLightmap(f1);
                return half4(f2,1)*atten;
            }
            ENDCG
        }
    }
}
