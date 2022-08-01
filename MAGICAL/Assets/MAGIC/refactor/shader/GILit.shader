Shader "MAGIC/GIlit"
{
    Properties
    {
        _MainTex ("Main texture", 2D) = "black" {}
        _GI ("GI lightmap", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragmentProcessing
            // make fog work
            #pragma multi_compile_fog

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _GI;
            float4 _MainTex_ST;
            float4 _GI_ST;

            rasterData vert (meshData input)
            {
                rasterData ouput;
                ouput.vertex = UnityObjectToClipPos(input.vertex);
                ouput.uv = TRANSFORM_TEX(input.uv, _MainTex);
                UNITY_TRANSFER_FOG(ouput,ouput.vertex);
                return ouput;
            }

            fixed4 fragmentProcessing (rasterData input) : SV_Target
            {
                // sample the texture
                fixed4 color = tex2D(_MainTex, input.uv);
                //sample displayGI texture
                fixed4 GI = tex2D(_GI, input.uv);
                
                //decode GI (16bit -> chromalum -> RGB)
                float luminance = IntrgbaToFloat32(float4(GI.xy,0,0));
                float3 c = YCoCgToRgb(float3(luminance,GI.z,GI.w));
                // GI = float4(c,1);

                //apply GI to texture
                color += GI;

                // apply fog
                // UNITY_APPLY_FOG(input.fogCoord, color);

                // return color;
                // return GI.a;
                return 1 - luminance*16;
            }
            ENDCG
        }
    }
}
