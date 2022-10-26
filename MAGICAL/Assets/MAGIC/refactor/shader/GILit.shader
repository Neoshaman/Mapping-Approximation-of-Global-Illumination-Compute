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
                fixed4 GI = tex2D(_GI, input.uv);   return GI;
                
                //decode GI (16bit -> chromalum -> RGB)
                float luminance = IntrgbaToFloat32(float4(GI.xy,0,0));      //return luminance*16;
                float3 lighting = YCoCgToRgb(float3(luminance,GI.z,GI.w));         return  float4(lighting,1);

                GI = float4(lighting,1);
                color += GI;
                return color;
                // return GI.a;
            }
            ENDCG
        }
    }
}
